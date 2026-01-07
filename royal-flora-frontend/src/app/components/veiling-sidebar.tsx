"use client";
import React, { useEffect, useState } from "react";
import "../../styles/veiling-sidebar.css";
import { getSessionData } from "../utils/sessionService";
import { getAuthHeaders } from "../utils/auth";
import { API_BASE_URL } from "../config/api";

interface SidebarProps {
  onReset: () => void;
  onStop: () => void;
  locationName: string;
  verkoopPrijs?: number | null;
}

interface VeilingDTO {
  id: number;
  naam: string;
  beschrijving: string;
  locatie?: string;
  status?: number;
}

export default function Sidebar({
  onReset,
  onStop,
  locationName,
  verkoopPrijs,
}: SidebarProps) {
  const [products, setProducts] = useState<VeilingDTO[]>([]);
  const [fetchError, setFetchError] = useState<string | null>(null);
  useEffect(() => {
    const load = async () => {
      setFetchError(null);
      const base = `${API_BASE_URL}/api/Products/Veiling`;
      if (!locationName) {
        setProducts([]);
        return;
      }

      try {
        const res = await fetch(base);
        const text = await res.text();
        if (!res.ok) {
          console.error(`Veiling fetch failed: ${res.status}`, text);
          setProducts([]);
          setFetchError(text || `HTTP error ${res.status}`);
          return;
        }

        try {
          const data = JSON.parse(text);
          if (!Array.isArray(data)) {
            setProducts([]);
            setFetchError('Unexpected response shape');
            return;
          }

          const filtered = data.filter(
            (p) =>
              p.locatie?.toLowerCase() === locationName.toLowerCase() &&
              p.status === 3
          );

          setProducts(filtered);
        } catch (parseError) {
          console.error('Error parsing JSON response:', parseError, 'Response text:', text);
          setProducts([]);
          setFetchError('Invalid JSON response from server');
        }
      } catch (err: any) {
        console.error('Error fetching product:', err);
        setProducts([]);
        setFetchError(err?.message ?? 'Network error');
      }
    };

    load();
  }, [locationName]);

  const product = products[0];

  const [soldMatches, setSoldMatches] = useState<SoldItem[] | null>(null);
  const [showSoldPopup, setShowSoldPopup] = useState(false);
  const [soldMessage, setSoldMessage] = useState<string | null>(null);

  interface SoldItem {
    IdProduct: number;
    ProductNaam: string;
    VerkoopPrijs: number;
    Aantal?: number | null;
  }

  const handleKoop = async () => {
    if (!product) return;
    const prijs = verkoopPrijs ?? 0;
    const roundedPrijs = Number(prijs.toFixed(2));

    try {
      const response = await fetch(
        `${API_BASE_URL}/api/Products/${product.id}/koop`,
        {
          method: "PATCH",
          headers: {
            "Content-Type": "application/json",
            ...getAuthHeaders()
          },
          body: JSON.stringify({
            verkoopPrijs: roundedPrijs,
          }),
          credentials: "include",
        }
      );

      if (response.ok) {
        console.log("Product gekocht!");
        window.location.reload();
      } else {
        console.error("product niet gekocht:", response.status);
      }
    } catch (err) {
      console.error("Error tijdens koop:", err);
    }
  };

  const fetchSoldMatches = async () => {
    setSoldMessage(null);
    try {
      const res = await fetch(`${API_BASE_URL}/api/Products/VeilingSoldMatches?locatie=${encodeURIComponent(locationName)}`);
      if (res.status === 404) {
        // No active veiling for this location
        setSoldMatches([]);
        setSoldMessage('Geen actieve veiling voor deze locatie.');
        setShowSoldPopup(true);
        return;
      }

      if (!res.ok) {
        console.error('Failed to fetch sold matches', res.status);
        setSoldMatches([]);
        setSoldMessage('Fout bij ophalen van verkochte items.');
        setShowSoldPopup(true);
        return;
      }

      const data = await res.json();
      const raw = data?.soldProducts || data?.SoldProducts || [];
      // Normalize keys to expected shape
      const normalized: SoldItem[] = (Array.isArray(raw) ? raw : []).map((it: any) => ({
        IdProduct: it.idProduct ?? it.IdProduct ?? 0,
        ProductNaam: it.productNaam ?? it.ProductNaam ?? '',
        VerkoopPrijs: it.verkoopPrijs ?? it.VerkoopPrijs ?? 0,
        Aantal: it.aantal ?? it.Aantal ?? null,
      }));

      setSoldMatches(normalized);
      setShowSoldPopup(true);
    } catch (err) {
      console.error('Error fetching sold matches', err);
      setSoldMatches([]);
      setSoldMessage('Onverwachte fout bij ophalen.');
      setShowSoldPopup(true);
    }
  };

  return (
    <div className="sidebar">
      <div className="sidebar-top">
        {fetchError ? (
          <p className="error">{fetchError}</p>
        ) : product ? (
          <>
            <p>Naam: {product.naam}</p>
            <br />
            <p>Beschrijving: {product.beschrijving}</p>
          </>
        ) : null}
      </div>
      <div className="sidebar-bottom">
        <button onClick={handleKoop}>Koop</button>
        <button onClick={onReset}>Reset</button>
      </div>

      <div className="sidebar-actions">
        <button onClick={fetchSoldMatches}>Vorige verkochte items</button>
      </div>

      {showSoldPopup && (
        <div className="sold-popup-overlay" onClick={() => setShowSoldPopup(false)}>
          <div className="sold-popup" onClick={(e) => e.stopPropagation()}>
            <h3>Vorige verkochte items voor: {product?.naam ?? locationName}</h3>
            <button className="close" onClick={() => setShowSoldPopup(false)}>Sluit</button>
            <div className="sold-list">
              {soldMessage ? (
                <p>{soldMessage}</p>
              ) : soldMatches && soldMatches.length > 0 ? (
                <ul>
                  {soldMatches.map((s) => (
                    <li key={s.IdProduct}>
                      <strong>{s.ProductNaam}</strong> — Aantal: {s.Aantal ?? '-'} — Prijs: €{s.VerkoopPrijs.toFixed(2)}
                    </li>
                  ))}
                </ul>
              ) : (
                <p>Geen verkochte items gevonden.</p>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
