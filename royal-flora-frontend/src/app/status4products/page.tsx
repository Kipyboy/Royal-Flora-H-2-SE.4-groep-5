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
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [naamFilter, setNaamFilter] = useState('');
  const [filteredProducts, setFilteredProducts] = useState<Status4Product[]>([]);

  useEffect(() => {
    fetchProducts();
  }, []);

  useEffect(() => {
    // Filter products based on the input (front-end filtering for responsiveness)
    const filtered = products.filter((product) =>
      product.productNaam.toLowerCase().includes(naamFilter.toLowerCase())
    );
    setFilteredProducts(filtered);
  }, [naamFilter, products]);

  const fetchProducts = async (filter?: string) => {
    try {
      setLoading(true);
      setError(null);

      const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5156';
      const url = new URL(`${apiUrl}/api/products/Status4Products`);

      if (filter) {
        url.searchParams.append('naamFilter', filter);
      }

      const response = await fetch(url.toString());

      if (!response.ok) {
        throw new Error(`Failed to fetch products: ${response.status}`);
      }

      const data = await response.json();
      setProducts(data);
      setFilteredProducts(data);
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
  };

  const handleSearchWithAPI = async () => {
    // Optional: fetch from backend with filter (for server-side filtering)
    if (naamFilter.trim()) {
      await fetchProducts(naamFilter);
    } else {
      await fetchProducts();
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

        {/* Results Count */}
        <p className="text-gray-600 mb-4">
          {filteredProducts.length} product{filteredProducts.length !== 1 ? 's' : ''} found
        </p>

        {/* Products Table */}
        {filteredProducts.length > 0 ? (
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
            <p className="text-gray-600">
              {naamFilter ? 'No products found matching your filter.' : 'No sold products available.'}
            </p>
          </div>
        )}

        {/* Back Link */}
        <Link href="/homepage" className="mt-6 inline-block text-blue-500 hover:text-blue-700 underline">
          ← Back to Home
        </Link>
      </div>
    </div>
  );
}
