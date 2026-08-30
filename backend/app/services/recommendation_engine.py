"""Recommendation engine for BookSage-AI."""
from typing import Any

from app.core.config import Config
from app.core.logger import logger
from app.services.collaborative_model import CollaborativeFilteringModel
from app.services.content_model import ContentBasedModel
from app.services.data_loader import DataLoader
from app.services.data_preprocessor import DataPreprocessor
from app.services.hybrid_model import HybridRecommendationModel
from app.services.model_manager import ModelManager


class RecommendationEngine:
    """Main recommendation engine combining all models."""

    def __init__(self):
        """Initialize the recommendation engine."""
        self.cf_model: CollaborativeFilteringModel | None = None
        self.cb_model: ContentBasedModel | None = None
        self.hybrid_model: HybridRecommendationModel | None = None
        self.processed_data: dict | None = None
        self.model_manager = ModelManager()
        self.is_trained: bool = False

    def train_models(self) -> bool:
        """
        Train all recommendation models.

        Returns:
            True if training successful, False otherwise
        """
        logger.info("=" * 60)
        logger.info("Starting model training...")
        logger.info("=" * 60)

        # Load data
        logger.info("1. Loading data...")
        books = DataLoader.load_books()
        users = DataLoader.load_users()
        ratings = DataLoader.load_ratings()

        if any(data is None for data in [books, users, ratings]):
            logger.error("Failed to load data")
            return False

        # Preprocess data
        logger.info("2. Preprocessing data...")
        preprocessor = DataPreprocessor(books, users, ratings)
        preprocessor.filter_active_users()
        preprocessor.merge_ratings_with_books()
        preprocessor.filter_popular_books()
        preprocessor.prepare_content_features()

        self.processed_data = preprocessor.get_processed_data()

        # Train collaborative filtering model
        logger.info("3. Training collaborative filtering model...")
        self.cf_model = CollaborativeFilteringModel()
        self.cf_model.train(self.processed_data["final_rating"])

        # Train content-based model
        logger.info("4. Training content-based model...")
        self.cb_model = ContentBasedModel()
        self.cb_model.train(self.processed_data["books_content"])

        # Create hybrid model
        logger.info("5. Creating hybrid model...")
        self.hybrid_model = HybridRecommendationModel(
            self.cf_model, self.cb_model
        )

        # Save models
        logger.info("6. Saving models...")
        if self.model_manager.save_models(
            self.cf_model, self.cb_model, self.processed_data
        ):
            self.is_trained = True
            logger.info("=" * 60)
            logger.info("Model training completed successfully!")
            logger.info("=" * 60)
            return True
        else:
            logger.error("Failed to save models")
            return False

    def load_trained_models(self) -> bool:
        """
        Load pre-trained models.

        Returns:
            True if loading successful, False otherwise
        """
        logger.info("Checking for existing trained models...")

        if not self.model_manager.models_exist():
            logger.warning("No trained models found")
            return False

        loaded_data = self.model_manager.load_models()

        if loaded_data:
            self.cf_model = loaded_data["cf_model"]
            self.cb_model = loaded_data["cb_model"]
            self.hybrid_model = loaded_data["hybrid_model"]
            self.processed_data = {
                "books_content": loaded_data["books_content"],
                "final_rating": loaded_data["final_rating"],
                "books": loaded_data["books"]
            }
            self.is_trained = True
            logger.info("Models loaded successfully!")
            return True

        logger.error("Failed to load models")
        return False

    def get_recommendations(
        self,
        book_title: str,
        method: str = "hybrid",
        top_n: int = Config.DEFAULT_TOP_N
    ) -> list[dict[str, Any]]:
        """
        Get recommendations using specified method.

        Args:
            book_title: Title of book to get recommendations for
            method: Recommendation method ('collaborative', 'content', 'hybrid')
            top_n: Number of recommendations to return

        Returns:
            List of recommendation dictionaries
        """
        if not self.is_trained:
            logger.warning("Models not trained or loaded")
            return []

        if method == "collaborative":
            return self.cf_model.get_recommendations(
                book_title,
                self.processed_data["books_content"],
                self.processed_data["books"],
                top_n
            )
        elif method == "content":
            return self.cb_model.get_recommendations(
                book_title,
                self.processed_data["books_content"],
                top_n
            )
        elif method == "hybrid":
            return self.hybrid_model.get_recommendations(
                book_title,
                self.processed_data["books_content"],
                self.processed_data["books"],
                top_n=top_n
            )
        else:
            logger.warning(
                "Invalid method. Use 'collaborative', 'content', or 'hybrid'"
            )
            return []

    def get_available_books(self, limit: int | None = None) -> list[str]:
        """
        Get list of all available books for recommendations.

        Args:
            limit: Maximum number of books to return

        Returns:
            List of book titles
        """
        if not self.is_trained:
            logger.warning("Models not trained or loaded")
            return []

        books = self.processed_data["books_content"]["title"].unique().tolist()
        if limit:
            return books[:limit]
        return books

    def search_books(self, query: str, limit: int = 10) -> list[dict[str, Any]]:
        """
        Search for books by title.

        Args:
            query: Search query string
            limit: Maximum number of results

        Returns:
            List of matching book dictionaries
        """
        if not self.is_trained:
            logger.warning("Models not trained or loaded")
            return []

        books = self.processed_data["books_content"]
        matching_books = books[
            books["title"].str.contains(query, case=False, na=False)
        ]

        results = []
        for _, book in matching_books.head(limit).iterrows():
            img_url = book["img_url"]
            if not isinstance(img_url, str) or not img_url.startswith("http"):
                img_url = Config.DEFAULT_IMAGE_URL

            results.append({
                "title": book["title"],
                "author": book["author"],
                "year": book["year"],
                "publisher": book["publisher"],
                "image_url": img_url
            })

        return results

    def get_book_info(self, book_title: str) -> dict[str, Any] | None:
        """
        Get detailed information about a specific book.

        Args:
            book_title: Title of the book

        Returns:
            Book info dictionary or None if not found
        """
        if not self.is_trained:
            return None

        book_info = self.processed_data["books_content"][
            self.processed_data["books_content"]["title"] == book_title
        ]

        if book_info.empty:
            return None

        book = book_info.iloc[0]
        img_url = book["img_url"]
        if not isinstance(img_url, str) or not img_url.startswith("http"):
            img_url = Config.DEFAULT_IMAGE_URL

        return {
            "title": book["title"],
            "author": book["author"],
            "year": book["year"],
            "publisher": book["publisher"],
            "image_url": img_url
        }

    def get_popular_books(self, limit: int = 12) -> list[dict[str, Any]]:
        """
        Get popular books based on rating count.

        Args:
            limit: Number of popular books to return

        Returns:
            List of popular book dictionaries
        """
        if not self.is_trained:
            return []

        popular_titles = (
            self.processed_data["final_rating"]
            .groupby("title")["rating"]
            .count()
            .sort_values(ascending=False)
            .head(limit)
            .index.tolist()
        )

        books_data = []
        for title in popular_titles:
            book_info = self.processed_data["books_content"][
                self.processed_data["books_content"]["title"] == title
            ]
            if book_info.empty:
                book_info = self.processed_data["books"][
                    self.processed_data["books"]["title"] == title
                ]
                if book_info.empty:
                    continue

            book = book_info.iloc[0]
            img_url = book["img_url"]
            if not isinstance(img_url, str) or not img_url.startswith("http"):
                img_url = Config.DEFAULT_IMAGE_URL

            books_data.append({
                "title": title,
                "author": book["author"],
                "year": book["year"],
                "publisher": book["publisher"],
                "image_url": img_url
            })

        return books_data
