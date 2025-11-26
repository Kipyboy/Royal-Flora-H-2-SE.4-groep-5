"use client";
import React, {useEffect, useState} from "react";
import '../../styles/veiling-sidebar.css';

interface SidebarProps {
  onReset: () => void;
  onStop: () => void;
  apiUrl: string;
}

interface VeilingDTO {
    id: number;
    naam: string;
    beschrijving: string;
}

export default function Sidebar({ onReset, onStop, apiUrl}: SidebarProps) {

  const [products, setProducts] = useState<VeilingDTO[]>([]);

  useEffect(() => {
    fetch("http://localhost:5156/api/Products1/Veiling") 
      .then(res => res.json())
      .then(data => setProducts(data))
      .catch(err => console.error("Error fetching product:", err));
  }, []);

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