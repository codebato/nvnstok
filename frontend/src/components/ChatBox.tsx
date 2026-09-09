import { useState } from 'react';
import { askAssistant } from '../api/assistantApi';

export default function ChatBox() {
  const [question, setQuestion] = useState('');
  const [answer, setAnswer] = useState('');
  const [loading, setLoading] = useState(false);

  const handleAsk = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!question.trim()) return;

    setLoading(true);
    setAnswer('');
    try {
      const response = await askAssistant(question);
      setAnswer(response.answer);
    } catch (err) {
      setAnswer('Bir hata oluştu, tekrar dener misin?');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="chat-box">
      <form onSubmit={handleAsk}>
        <input
          placeholder="Örn: Hangi ürünler kritik stokta?"
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
        />
        <button type="submit" disabled={loading}>
          {loading ? 'Sorgulanıyor...' : 'Sor'}
        </button>
      </form>
      {answer && <div className="chat-answer">{answer}</div>}
    </div>
  );
}