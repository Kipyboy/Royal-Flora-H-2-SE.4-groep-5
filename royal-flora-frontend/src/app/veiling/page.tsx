"use client";
import React, { useEffect, useRef, useState } from "react";
import "../../styles/Root.css";
import "../../styles/Veiling.css";

const DEFAULT_MS = 5 * 60 * 1000;
const STORAGE_KEY = "veiling_end_ts";
const DISPLAY_ID = "veiling-timer";

const PHOTOS = [
  "https://picsum.photos/id/1015/1155/615",
  "https://picsum.photos/id/1016/600/400",
  "https://picsum.photos/id/1018/600/400",
  "https://picsum.photos/id/1020/600/400",
  "https://picsum.photos/id/1024/600/400",
  "https://picsum.photos/id/1025/600/400",
  "https://picsum.photos/id/1035/600/400"
];

function formatMs(ms: number) {
  if (ms <= 0) return "00:00";
  const totalSec = Math.floor(ms / 1000);
  const min = Math.floor(totalSec / 60);
  const sec = totalSec % 60;
  return String(min).padStart(2, "0") + ":" + String(sec).padStart(2, "0");
}

function readEndFromStorage() {
  if (typeof window === "undefined") return null;
  const v = localStorage.getItem(STORAGE_KEY);
  const n = v ? parseInt(v, 10) : NaN;
  if (!isFinite(n)) return null;
  return n;
}

function writeEndToStorage(ts: number) {
  if (typeof window === "undefined") return;
  localStorage.setItem(STORAGE_KEY, String(ts));
}



export default function VeilingPage() {
  const [endTs, setEndTs] = useState<number | null>(null);
  const [remaining, setRemaining] = useState(DEFAULT_MS);
  const [viewerUrl, setViewerUrl] = useState<string | null>(null);
  const [clock, setClock] = useState("--:--:--");
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  // Timer logic
  useEffect(() => {
    function now() {
      return Date.now();
    }
    function updateRemaining() {
      if (endTs) {
        setRemaining(Math.max(0, endTs - now()));
      }
    }
    if (endTs) {
      updateRemaining();
      if (!intervalRef.current) {
        intervalRef.current = setInterval(updateRemaining, 250);
      }
    }
    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [endTs]);

  // Storage sync
  useEffect(() => {
    function onStorage(e: StorageEvent) {
      if (e.key !== STORAGE_KEY) return;
      const stored = readEndFromStorage();
      setEndTs(stored && stored > Date.now() ? stored : null);
    }
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  // Init timer on mount
  useEffect(() => {
    const stored = readEndFromStorage();
    if (stored && stored > Date.now()) {
      setEndTs(stored);
    } else {
      const newEnd = Date.now() + DEFAULT_MS;
      setEndTs(newEnd);
      writeEndToStorage(newEnd);
    }
    // eslint-disable-next-line
  }, []);

  // Clock logic
  useEffect(() => {
    function updateClock() {
      const now = new Date();
      setClock(
        now.toLocaleTimeString("nl-NL", { hour12: false })
      );
    }
    updateClock();
    const id = setInterval(updateClock, 1000);
    return () => clearInterval(id);
  }, []);

  // Timer reset/stop handlers
  function handleReset() {
    const newEnd = Date.now() + DEFAULT_MS;
    setEndTs(newEnd);
    writeEndToStorage(newEnd);
  }
  function handleStop() {
    setEndTs(null);
    writeEndToStorage(0);
    setRemaining(0);
  }

  return (
    <>
      <nav>
        <a href="/homepage" className="skip-link">Home</a>
        <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/92/Royal_FloraHolland_Logo.svg/1200px-Royal_FloraHolland_Logo.svg.png" alt="Royal_FloraHolland_Logo" />
        <a className="pfp-container" href="">
          <img src="https://www.pikpng.com/pngl/m/80-805068_my-profile-icon-blank-profile-picture-circle-clipart.png" alt="Profiel-foto" />
        </a>
      </nav>
      <div id={DISPLAY_ID}>{formatMs(remaining)}</div>
      <div className="main-content">
        <div id="viewer" className={viewerUrl ? "" : "hidden"}>
          {viewerUrl && <img src={viewerUrl} alt="Foto" />}
        </div>
      </div>
      <div className="gallery">
        <div id="photo-strip">
          {PHOTOS.map((url) => (
            <img
              key={url}
              src={url}
              alt="Foto"
              onClick={() => setViewerUrl(url)}
              style={{ cursor: "pointer" }}
            />
          ))}
        </div>
      </div>
      <div className="sidebar">
        <div className="sidebar-top">
          <p>Naam:</p>
          <p>Beschrijving:</p>
        </div>
        <div className="sidebar-bottom">
          <p>Prijs: </p>
          <button onClick={handleStop}>Koop</button>
          <div id="huidige-tijd">{clock}</div>
        </div>
      </div>
    </>
  );
}
