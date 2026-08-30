"""Pydantic models for request/response schemas."""
from typing import List, Optional

from pydantic import BaseModel, Field


class BookInfo(BaseModel):
    """Book information schema."""

    title: str
    author: str
    year: Optional[str] = None
    publisher: Optional[str] = None
    image_url: str = Field(alias="image_url")

    class Config:
        """Pydantic config."""

        populate_by_name = True


class BookRecommendation(BaseModel):
    """Book recommendation schema."""

    title: str
    author: str
    year: Optional[str] = None
    publisher: Optional[str] = None
    image_url: str
    score: float
    type: str  # 'collaborative', 'content', 'hybrid'


class RecommendRequest(BaseModel):
    """Recommendation request schema."""

    book_title: str
    method: str = "hybrid"  # 'collaborative', 'content', 'hybrid'


class SearchResult(BaseModel):
    """Search result schema."""

    title: str
    author: str
    image_url: str


class SearchResponse(BaseModel):
    """Search response schema."""

    results: List[SearchResult]
