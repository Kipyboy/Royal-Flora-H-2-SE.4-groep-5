"use client";
import React, { useEffect, useState } from "react";
import "../../styles/veiling-sidebar.css";
import { getSessionData } from "../utils/sessionService";
import { getAuthHeaders } from "../utils/auth";

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

  useEffect(() => {
    const base = "http://localhost:5156/api/Products/Veiling";
    if (!locationName) {
      setProducts([]);
      return;
    }

    fetch(base)
      .then((res) => {
        if (!res.ok) {
          throw new Error(`HTTP error! status: ${res.status}`);
        }
        return res.text();
      })
      .then((text) => {
        try {
          const data = JSON.parse(text);
          if (!Array.isArray(data)) {
            setProducts([]);
            return;
          }

          const filtered = data.filter(
            (p) =>
              p.locatie?.toLowerCase() === locationName.toLowerCase() &&
              p.status === 3
          );

          setProducts(filtered);
        } catch (parseError) {
          console.error("Error parsing JSON response:", parseError, "Response text:", text);
          setProducts([]);
        }
      })
      .catch((err) => {
        console.error("Error fetching product:", err);
        setProducts([]);
      });
  }, [locationName]);

  const product = products[0];

  const handleKoop = async () => {
    if (!product) return;
    const prijs = verkoopPrijs ?? 0;
    const roundedPrijs = Number(prijs.toFixed(2));

    try {
      const response = await fetch(
        `http://localhost:5156/api/Products/${product.id}/koop`,
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
        await fetch(`/api/Products/Advance?locatie=${locationName}`, {
          method: "POST",
          ...getAuthHeaders()
        });
        window.location.reload();
      } else {
        console.error("product niet gekocht:", response.status);
      }
    } catch (err) {
      console.error("Error tijdens koop:", err);
    }
  };

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
        <button onClick={handleKoop}>Koop</button>
        <button onClick={onReset}>Reset</button>
      </div>
    </div>
  );
}
