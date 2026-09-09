import { useState, useEffect } from 'react';
import { getAllProducts, createProduct, type Product } from '../api/productsApi';
import { useAuth } from '../context/AuthContext';
import ProductList from '../components/ProductList';
import ProductForm from '../components/ProductForm';
import ChatBox from '../components/ChatBox';

export default function Dashboard() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const { email, logout } = useAuth();


  const fetchProducts = async () => {
    try {
      const data = await getAllProducts();
      setProducts(data);
    } catch (err) {
      console.error('Ürünler yüklenemedi:', err);
    } finally {
      setLoading(false);
    }
  };


  useEffect(() => {
    fetchProducts();
  }, []);

 
  const handleProductCreated = async (sku: string, name: string, initialStock: number, lowStockThreshold: number) => {
    await createProduct({ sku, name, initialStock, lowStockThreshold });
    await fetchProducts(); 
  };

  return (
    <div className="dashboard">
      <header>
        <h1>NvnStok Dashboard</h1>
        <div>
          <span>{email}</span>
          <button onClick={logout}>Çıkış Yap</button>
        </div>
      </header>

      <main>
        <section>
          <h2>Ürünlerim</h2>
          <ProductForm onProductCreated={handleProductCreated} />
          {loading ? <p>Yükleniyor...</p> : <ProductList products={products} />}
        </section>

        <section>
          <h2>AI Asistan</h2>
          <ChatBox />
        </section>
      </main>
    </div>
  );
}