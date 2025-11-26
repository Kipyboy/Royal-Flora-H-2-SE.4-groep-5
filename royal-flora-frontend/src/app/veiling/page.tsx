"use client";
import React, { useEffect, useState } from "react";
import '../../styles/Veiling.css';
import Topbar from "../components/Topbar";
import Clock from "../components/clock";
import VeilingSidebar from "../components/veiling-sidebar";

const configs = {
  a: { apiUrl: "/api/products?location=A", title: "Auction A" },
  b: { apiUrl: "/api/products?location=B", title: "Auction B" },
  c: { apiUrl: "/api/products?location=C", title: "Auction C" },
  d: { apiUrl: "/api/products?location=D", title: "Auction D" },
};

const DEFAULT_MS =  10 * 1000;
const STORAGE_KEY = "veiling_end_ts";

function readEndFromStorage() {
  const v = localStorage.getItem(STORAGE_KEY);
  const n = v ? parseInt(v, 10) : NaN;
  return isFinite(n) ? n : null;
}

function writeEndToStorage(ts: number) {
  localStorage.setItem(STORAGE_KEY, String(ts));
}

export default function VeilingPage({ searchParams }: { searchParams: { loc?: string } }) {
  const location = searchParams.loc ?? "a"; // default to A
  const config = configs[location as keyof typeof configs];

  const [endTs, setEndTs] = useState<number | null>(null);

  useEffect(() => {
  const stored = readEndFromStorage();
  const newEnd = stored && stored > Date.now() ? stored : Date.now() + DEFAULT_MS;
  setEndTs(newEnd);
  writeEndToStorage(newEnd);
  }, [DEFAULT_MS]);

  useEffect(() => {
    const onStorage = (e: StorageEvent) => {
      if (e.key === STORAGE_KEY) {
        const stored = readEndFromStorage();
        setEndTs(stored && stored > Date.now() ? stored : null);
      }
    };
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  const handleReset = () => {
    const newEnd = Date.now() + DEFAULT_MS;
    setEndTs(newEnd);
    writeEndToStorage(newEnd);
  };

  const handleStop = () => setEndTs(null);

  return (
    <div className="veiling-page">
      <Topbar currentPage="Veiling" useSideBar={false} />
      <div className="clock-container">
        <Clock endTs={endTs} durationMs={DEFAULT_MS} />
      </div>
      <VeilingSidebar onReset={handleReset} onStop={handleStop} apiUrl={config.apiUrl}/>
    </div>
  );
}