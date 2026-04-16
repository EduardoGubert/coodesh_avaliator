import { useState, type FormEvent } from 'react';
import type { OrderRequest, OrderResponse, ApiErrorResponse } from '../types/order';

const SYMBOLS = ['PETR4', 'VALE3', 'VIIA4'];
const SIDES = [
  { value: 'buy', label: 'Compra' },
  { value: 'sell', label: 'Venda' },
];

export default function OrderForm() {
  const [symbol, setSymbol] = useState(SYMBOLS[0]);
  const [side, setSide] = useState('buy');
  const [quantity, setQuantity] = useState('');
  const [price, setPrice] = useState('');
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<OrderResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setResult(null);
    setError(null);

    const qty = parseInt(quantity, 10);
    const prc = parseFloat(price);

    if (isNaN(qty) || qty <= 0 || qty >= 100_000) {
      setError('Quantidade deve ser um inteiro positivo menor que 100.000');
      return;
    }
    if (isNaN(prc) || prc <= 0 || prc >= 1_000) {
      setError('Preco deve ser positivo e menor que 1.000');
      return;
    }
    if (Math.round(prc * 100) !== prc * 100) {
      setError('Preco deve ser multiplo de 0.01');
      return;
    }

    const request: OrderRequest = { symbol, side, quantity: qty, price: prc };

    setLoading(true);
    try {
      const res = await fetch('/api/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request),
      });

      if (!res.ok) {
        const body: ApiErrorResponse = await res.json();
        setError(body.details ? body.details.join('; ') : body.error);
        return;
      }

      const data: OrderResponse = await res.json();
      setResult(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro de conexao');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="order-form-container">
      <h1>Order Generator</h1>
      <form onSubmit={handleSubmit} className="order-form">
        <div className="form-group">
          <label htmlFor="symbol">Simbolo</label>
          <select id="symbol" value={symbol} onChange={(e) => setSymbol(e.target.value)}>
            {SYMBOLS.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label htmlFor="side">Lado</label>
          <select id="side" value={side} onChange={(e) => setSide(e.target.value)}>
            {SIDES.map((s) => (
              <option key={s.value} value={s.value}>{s.label}</option>
            ))}
          </select>
        </div>

        <div className="form-group">
          <label htmlFor="quantity">Quantidade</label>
          <input
            id="quantity"
            type="number"
            min="1"
            max="99999"
            step="1"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
            placeholder="1 - 99999"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="price">Preco</label>
          <input
            id="price"
            type="number"
            min="0.01"
            max="999.99"
            step="0.01"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            placeholder="0.01 - 999.99"
            required
          />
        </div>

        <button type="submit" disabled={loading}>
          {loading ? 'Enviando...' : 'Enviar Ordem'}
        </button>
      </form>

      {error && (
        <div className="result error">
          <h3>Erro</h3>
          <p>{error}</p>
        </div>
      )}

      {result && (
        <div className="result success">
          <h3>Ordem Executada</h3>
          <table>
            <tbody>
              <tr><td>Order ID</td><td>{result.orderId}</td></tr>
              <tr><td>Exec ID</td><td>{result.execId}</td></tr>
              <tr><td>ClOrdId</td><td>{result.clOrdId}</td></tr>
              <tr><td>Simbolo</td><td>{result.symbol}</td></tr>
              <tr><td>Lado</td><td>{result.side}</td></tr>
              <tr><td>Quantidade</td><td>{result.quantity}</td></tr>
              <tr><td>Preco</td><td>{result.price.toFixed(2)}</td></tr>
              <tr><td>Status</td><td>{result.status}</td></tr>
              {result.message && <tr><td>Mensagem</td><td>{result.message}</td></tr>}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
