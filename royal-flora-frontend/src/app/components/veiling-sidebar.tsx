"use client";
import React, {useEffect, useState} from "react";
import '../../styles/veiling-sidebar.css';

interface SidebarProps {
  onReset: () => void;
  onStop: () => void;
  locationName: string;
}

interface VeilingDTO {
    id: number;
    naam: string;
    beschrijving: string;
    locatie?: string;
}

export default function Sidebar({ onReset, onStop, locationName }: SidebarProps) {

  const [products, setProducts] = useState<VeilingDTO[]>([]);

  useEffect(() => {
    const base = "http://localhost:5156/api/Products/Veiling";

    fetch(base)
      .then(res => res.json())
      .then((data: VeilingDTO[]) => {
        if (!Array.isArray(data)) {
          setProducts([]);
          return;
        }
        const filtered = data.filter(p => (p.locatie ?? "").toString().toLowerCase() === locationName.toLowerCase());
        setProducts(filtered);
      })
      .catch(err => {
        console.error("Error fetching product:", err);
        setProducts([]);
      });
  }, [locationName]);

  const product = products[0];

  return (
    <div className="sidebar">
      <div className="sidebar-top">
        {product ? (
          <>
            <p>Naam: {product.naam}</p>
            <br />
            <p>Beschrijving: {product.beschrijving}</p>
          </>
        ) : null}
      </div>
      <div className="sidebar-bottom">
        <button onClick={onStop}>Koop</button>
        <button onClick={onReset}>Reset</button>
      </div>
    </div>
  );
}