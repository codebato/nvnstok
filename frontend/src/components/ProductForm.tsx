import { useState } from 'react';

interface ProductFormProps {
  onProductCreated: (sku: string, name: string, initialStock: number, lowStockThreshold: number) => Promise<void>;
}

export default function ProductForm({ onProductCreated }: ProductFormProps) {
  const [sku, setSku] = useState('');
  const [name, setName] = useState('');
  const [initialStock, setInitialStock] = useState(0);
  const [lowStockThreshold, setLowStockThreshold] = useState(5);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await onProductCreated(sku, name, initialStock, lowStockThreshold);
      // Başarılı eklemeden sonra formu temizliyoruz
      setSku('');
      setName('');
      setInitialStock(0);
      setLowStockThreshold(5);
    } catch (err) {
      alert('Ürün eklenemedi. SKU zaten kullanılıyor olabilir.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="product-form">
      <input placeholder="SKU" value={sku} onChange={(e) => setSku(e.target.value)} required />
      <input placeholder="Ürün Adı" value={name} onChange={(e) => setName(e.target.value)} required />
      <input
        type="number"
        placeholder="Başlangıç Stoğu"
        value={initialStock}
        onChange={(e) => setInitialStock(Number(e.target.value))}
        min={0}
      />
      <input
        type="number"
        placeholder="Kritik Stok Eşiği"
        value={lowStockThreshold}
        onChange={(e) => setLowStockThreshold(Number(e.target.value))}
        min={0}
      />
      <button type="submit" disabled={submitting}>
        {submitting ? 'Ekleniyor...' : 'Ürün Ekle'}
      </button>
    </form>
  );
}