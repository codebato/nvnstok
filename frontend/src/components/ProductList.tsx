import type { Product } from '../api/productsApi';

interface ProductListProps {
  products: Product[];
}

export default function ProductList({ products }: ProductListProps) {
  if (products.length === 0) {
    return <p>Henüz ürün eklenmemiş.</p>;
  }

  return (
    <table className="product-table">
      <thead>
        <tr>
          <th>SKU</th>
          <th>İsim</th>
          <th>Stok</th>
          <th>Durum</th>
        </tr>
      </thead>
      <tbody>
        {products.map((product) => {
          const isLowStock = product.currentStock <= product.lowStockThreshold;
          return (
            <tr key={product.id}>
              <td>{product.sku}</td>
              <td>{product.name}</td>
              <td>{product.currentStock}</td>
              <td>
                {isLowStock ? (
                  <span className="badge badge-danger">Kritik Stok</span>
                ) : (
                  <span className="badge badge-success">Normal</span>
                )}
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}