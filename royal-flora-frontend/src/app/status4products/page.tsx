'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';

interface Status4Product {
  idProduct: number;
  productNaam: string;
  verkoopPrijs: number;
}

export default function Status4ProductsPage() {
  const [products, setProducts] = useState<Status4Product[]>([]);
  const [priceHistory, setPriceHistory] = useState<any | null>(null);
  // start not loading so the search bar is visible immediately
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [naamFilter, setNaamFilter] = useState('');
  const [filteredProducts, setFilteredProducts] = useState<Status4Product[]>([]);
  const [searched, setSearched] = useState(false);

  // No automatic fetch on mount. Wait for user to submit a product name.

  const fetchProducts = async (filter?: string) => {
    try {
      setLoading(true);
      setError(null);

      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5156';
      if (filter) {
        // Call new PriceHistory endpoint when user searches by name
        const url = new URL(`${apiUrl}/api/products/PriceHistory`);
        url.searchParams.append('naam', filter);
        const response = await fetch(url.toString());
        if (!response.ok) throw new Error(`Failed to fetch price history: ${response.status}`);
        const data = await response.json();
        setPriceHistory(data);
        setProducts([]);
        setFilteredProducts([]);
      } else {
        const url = new URL(`${apiUrl}/api/products/Status4Products`);
        const response = await fetch(url.toString());
        if (!response.ok) throw new Error(`Failed to fetch products: ${response.status}`);
        const data = await response.json();
        setProducts(data);
        setFilteredProducts(data);
        setPriceHistory(null);
      }
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      setError(`Error loading products: ${errorMessage}`);
      console.error('Error fetching products:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setNaamFilter(e.target.value);
    // mark that the current input hasn't been submitted yet
    setSearched(false);
  };

  const handleSearchWithAPI = async () => {
    // Optional: fetch from backend with filter (for server-side filtering)
    if (naamFilter.trim()) {
      setSearched(true);
      await fetchProducts(naamFilter);
    } else {
      setSearched(false);
      // If empty, clear any previous results and do nothing
      setPriceHistory(null);
      setProducts([]);
      setFilteredProducts([]);
      setError(null);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleSearchWithAPI();
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-100 p-6">
        <div className="max-w-4xl mx-auto">
          <h1 className="text-3xl font-bold text-gray-800 mb-6">Sold Products (Status 4)</h1>
          <p className="text-gray-600">Loading products...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-800 mb-6">Sold Products (Status 4)</h1>

        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-6">
            {error}
          </div>
        )}

        {/* Search Filter */}
        <div className="mb-6 bg-white p-4 rounded shadow">
          <label htmlFor="naamFilter" className="block text-sm font-medium text-gray-700 mb-2">
            Filter by Product Name (Case-Insensitive):
          </label>
          <div className="flex gap-2">
            <input
              id="naamFilter"
              type="text"
              value={naamFilter}
              onChange={handleSearchChange}
              onKeyPress={handleKeyPress}
              placeholder="Enter product name..."
              className="flex-1 px-4 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button
              onClick={handleSearchWithAPI}
              className="px-6 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition"
            >
              Search
            </button>
          </div>
        </div>

        {/* Results Count / Prompt */}
        {!naamFilter.trim() ? (
          <div className="bg-white p-6 rounded shadow text-center mb-4">
            <p className="text-gray-600">submit product name</p>
          </div>
        ) : searched ? (
          <p className="text-gray-600 mb-4">
            {priceHistory ? (priceHistory.items ?? priceHistory.Items ?? []).length : filteredProducts.length} product{(priceHistory ? (priceHistory.items ?? priceHistory.Items ?? []).length : filteredProducts.length) !== 1 ? 's' : ''} found
          </p>
        ) : null}

        {/* Products Table */}
        {priceHistory ? (
          <div className="bg-white rounded shadow p-4">
            <h2 className="text-lg font-semibold mb-2">Prijsgeschiedenis (top 10 meest recente)</h2>
            <ul>
              {(priceHistory.items ?? priceHistory.Items ?? []).map((it: any) => (
                <li key={it.idProduct ?? it.IdProduct} className="py-2 border-b last:border-b-0">
                  <div className="flex justify-between">
                    <div>
                      <strong>{it.productNaam ?? it.ProductNaam}</strong>
                      <div className="text-sm text-gray-600">Aanvoerder: {it.aanvoerderNaam ?? it.AanvoerderNaam ?? '-'}</div>
                      <div className="text-sm text-gray-600">Datum: {it.soldDate ?? it.SoldDate ?? '-'}</div>
                    </div>
                    <div className="text-right">€{((it.verkoopPrijs ?? it.VerkoopPrijs) ?? 0).toFixed(2)}</div>
                  </div>
                </li>
              ))}
            </ul>
            <div className="mt-4 text-sm text-gray-700">Gemiddelde prijs (recent 10): €{(priceHistory.averageVerkoopPrijs ?? priceHistory.AverageVerkoopPrijs ?? 0).toFixed(2)}</div>
            <div className="mt-1 text-sm text-gray-700">Gemiddelde prijs (alle bestellingen): €{((priceHistory.overallAverageVerkoopPrijs ?? priceHistory.OverallAverageVerkoopPrijs) ?? 0).toFixed(2)}</div>
          </div>
        ) : searched ? (
          filteredProducts.length > 0 ? (
          <div className="bg-white rounded shadow overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-200">
                <tr>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Product ID</th>
                  <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Product Name</th>
                  <th className="px-6 py-3 text-right text-sm font-semibold text-gray-700">Sales Price (€)</th>
                </tr>
              </thead>
              <tbody>
                {filteredProducts.map((product, index) => (
                  <tr
                    key={product.idProduct}
                    className={index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}
                  >
                    <td className="px-6 py-4 text-sm text-gray-900">{product.idProduct}</td>
                    <td className="px-6 py-4 text-sm text-gray-900">{product.productNaam ?? '-'}</td>
                    <td className="px-6 py-4 text-sm text-right text-gray-900">
                      €{(product.verkoopPrijs ?? 0).toFixed(2)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          ) : (
            <div className="bg-white p-6 rounded shadow text-center">
              <p className="text-gray-600">No products found matching your filter.</p>
            </div>
          )
        ) : null}

        {/* Back Link */}
        <Link href="/homepage" className="mt-6 inline-block text-blue-500 hover:text-blue-700 underline">
          ← Back to Home
        </Link>
      </div>
    </div>
  );
}
