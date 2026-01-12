"use client";
import React, { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import '../../styles/Veiling.css';
import Topbar from "../components/Topbar";
import Clock from "../components/clock";
import VeilingSidebar from "../components/veiling-sidebar";
import { getUser } from "../utils/auth";
import { API_BASE_URL } from "../config/api";

const configs = {
  a: { locationName: "Naaldwijk", title: "Auction A" },
  b: { locationName: "Aalsmeer", title: "Auction B" },
  c: { locationName: "Rijnsburg", title: "Auction C" },
  d: { locationName: "Eelde", title: "Auction D" },
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

export default function VeilingPage({ searchParams }: { searchParams: Promise<{ loc?: string }> }) {
  const router = useRouter();
  const params = React.use(searchParams);
  const location = params.loc ?? "a"; 
  const config = configs[location as keyof typeof configs];

  const [endTs, setEndTs] = useState<number | null>(null);
  const [currentPrice, setCurrentPrice] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const currentUser = getUser();
    if (!currentUser) {
      router.push('/login');
      return;
    }
    setLoading(false);
  }, [router]);

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


  const handleStop = () => setEndTs(null);

  if (loading) {
    return <div>Loading...</div>;
  }

  const handleClockFinished = async () => {
    const res = await fetch(`${API_BASE_URL}/api/Products/Advance?locatie=${config.locationName}`,  { method: "POST" });
    console.log("Advance response:", res.status);
    window.location.reload();
  };


  return (
    <div className="veiling-page">
      <Topbar currentPage="Veiling" useSideBar={false} />
      <div className="clock-container">
        <Clock endTs={endTs} durationMs={DEFAULT_MS} onPriceChange={setCurrentPrice} locationName={config.locationName} onFinished={handleClockFinished}/>
      </div>
      <VeilingSidebar onStop={handleStop} locationName={config.locationName} verkoopPrijs={currentPrice} />
    </div>
  );
}