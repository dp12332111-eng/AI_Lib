import React, { useState, useEffect } from 'react';
import Background from './components/Background';
import HomeView from './views/HomeView';
import ResultsView from './views/ResultsView';
import Footer from './components/Footer';

function App() {
  const [popularBooks, setPopularBooks] = useState([]);
  const [recommendations, setRecommendations] = useState([]);
  const [selectedBook, setSelectedBook] = useState(null);
  const [loading, setLoading] = useState(false);
  const [view, setView] = useState('home'); // 'home' or 'results'
  const [method, setMethod] = useState('hybrid');
  const [bookTitle, setBookTitle] = useState('');

  useEffect(() => {
    fetch('/api/popular')
      .then(res => res.json())
      .then(data => setPopularBooks(data))
      .catch(err => console.error(err));
  }, []);

  const handleRecommend = async (title, selectedMethod) => {
    setLoading(true);
    setMethod(selectedMethod);
    setBookTitle(title);
    try {
      const formData = new FormData();
      formData.append('book_title', title);
      formData.append('method', selectedMethod);

      const resp = await fetch('/api/recommend', {
        method: 'POST',
        body: formData,
      });
      const data = await resp.json();
      setRecommendations(data.recommendations);
      setSelectedBook(data.selected_book);
      setView('results');

      window.scrollTo({ top: 0, behavior: 'smooth' });
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleBack = () => {
    setView('home');
    setRecommendations([]);
    setSelectedBook(null);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="relative min-h-screen text-text-primary bg-bg-body font-inter selection:bg-accent/30">
      <Background />

      {loading && (
        <div className="fixed inset-0 z-[100] bg-white/60 backdrop-blur-sm flex items-center justify-center">
          <div className="flex flex-col items-center gap-4">
            <span className="loading loading-spinner loading-lg text-accent"></span>
            <p className="text-accent font-bold animate-pulse uppercase tracking-widest text-xs">Finding matches...</p>
          </div>
        </div>
      )}

      <main className="relative z-10">
        {view === 'home' ? (
          <HomeView
            popularBooks={popularBooks}
            onRecommend={handleRecommend}
          />
        ) : (
          <ResultsView
            bookTitle={bookTitle}
            selectedBook={selectedBook}
            recommendations={recommendations}
            method={method}
            onBack={handleBack}
            onRecommend={handleRecommend}
          />
        )}
      </main>

      <Footer />
    </div>
  );
}

export default App;
