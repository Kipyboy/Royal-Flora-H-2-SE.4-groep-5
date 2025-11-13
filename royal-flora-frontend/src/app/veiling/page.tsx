"use client";
import React, { useEffect, useRef, useState } from "react";
//import "../../styles/Root.css";
import "../../styles/Veiling.css";
import Topbar from "../components/Topbar";

const DEFAULT_MS = 10 * 60 * 1000;
const STORAGE_KEY = "veiling_end_ts";

const PHOTOS = [
  {url: "https://s3-eu-west-1.amazonaws.com/assets.botanic.cam.ac.uk/wp-content/uploads/2024/04/fotografierende-Na6tH0QLg6Y-unsplash2-2000x1125.jpg", alt: "Een zonnebloem van dichtbij"},
  {url: "https://www.lunafloral.my/cdn/shop/articles/pexels-pixabay-54267.jpg?v=1724733421&width=2048", alt: "Een heleboel zonnebloemen van dichtbij"},
  {url: "https://marvel-b1-cdn.bc0a.com/f00000000218560/www.seedway.com/app/uploads/2021/03/Full-Sun-Takii-1.jpg", alt: "Een bos zonnebloemen in een vaas"},
  {url: "https://www.threshseed.com/cdn/shop/products/autumn-beauty-sunflower-mix-seeds-38696723939577.jpg?v=1700321468&width=2000", alt: "Een bos verkleurde zonnebloemen in een houten emmer"},
  {url: "https://www.wala.world/assets/images/6/Art-Sonnenblume-8b445530.jpg", alt: "Een zonnebloemveld"},
  {url: "https://www.almanac.com/sites/default/files/styles/or/public/image_nodes/sunflowers_0.jpg?itok=ak_rjbkJ", alt: "Nog een veld zonnebloemen, maar dichterbij"},
  {url: "https://bombayseeds.com/cdn/shop/files/Sunflower.jpg?v=1729234679", alt: "Een close up van een enorme zonnebloem"}
];

function formatMs(ms: number) {
  if (ms <= 0) return "00:00.000";
  const totalSec = Math.floor(ms / 1000);
  const min = Math.floor(totalSec / 60);
  const sec = totalSec % 60;
  const millis = ms % 1000;
  return `${String(min).padStart(2, "0")}:${String(sec).padStart(2, "0")}.${String(millis).padStart(3, "0")}`;
}

function readEndFromStorage() {
  const v = localStorage.getItem(STORAGE_KEY);
  const n = v ? parseInt(v, 10) : NaN;
  return isFinite(n) ? n : null;
}

function writeEndToStorage(ts: number) {
  localStorage.setItem(STORAGE_KEY, String(ts));
}

function PhotoRow({ photos }: { photos: { url: string; alt: string }[] }) {
  const [page, setPage] = useState(0);

  const photosPerPage = 4;
  const start = page * photosPerPage;
  const visible = photos.slice(start, start + photosPerPage);

  const hasPrev = page > 0;
  const hasNext = start + photosPerPage < photos.length;

  return (
    <div className="photo-box">
      <div className="photo-row">
        {visible.map((photo) => (
          <img key={photo.url} src={photo.url} alt={photo.alt} tabIndex={0} />
        ))}
      </div>

      {photos.length > photosPerPage && (
        <div className="photo-nav">
          <button onClick={() => setPage((p) => Math.max(0, p - 1))} disabled={!hasPrev}>
            ◀
          </button>
          <button onClick={() => setPage((p) => (hasNext ? p + 1 : p))} disabled={!hasNext}>
            ▶
          </button>
        </div>
      )}
    </div>
  );
}

export default function VeilingPage() {
  const [endTs, setEndTs] = useState<number | null>(null);
  const svgRef = useRef<SVGSVGElement | null>(null);

  useEffect(() => {
    const stored = readEndFromStorage();
    const newEnd = stored && stored > Date.now() ? stored : Date.now() + DEFAULT_MS;
    setEndTs(newEnd);
    writeEndToStorage(newEnd);
  }, []);

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

  useEffect(() => {
    if (!svgRef.current || !endTs) return;
    const svg = svgRef.current;
    svg.innerHTML = "";

    const radius = 200;
    const center = radius + 50;
    const startingPrice = 1000;
    const minPrice = 10;

    const circle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
    circle.setAttribute("cx", center.toString());
    circle.setAttribute("cy", center.toString());
    circle.setAttribute("r", radius.toString());
    circle.setAttribute("stroke", "#4CAF50");
    circle.setAttribute("stroke-width", "8");
    circle.setAttribute("fill", "white");
    svg.appendChild(circle);

    const tickCircles: SVGCircleElement[] = [];
    for (let i = 0; i < 100; i++) {
      const angle = (i / 100) * 2 * Math.PI - Math.PI / 2;
      const x = center + radius * Math.cos(angle);
      const y = center + radius * Math.sin(angle);
      const tick = document.createElementNS("http://www.w3.org/2000/svg", "circle");
      tick.setAttribute("cx", x.toString());
      tick.setAttribute("cy", y.toString());
      tick.setAttribute("r", "5");
      tick.setAttribute("fill", "#ccc");
      svg.appendChild(tick);
      tickCircles.push(tick);
    }

    const createText = (offsetY: number, fontSize: number) => {
      const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
      text.setAttribute("x", center.toString());
      text.setAttribute("y", (center + offsetY).toString());
      text.setAttribute("text-anchor", "middle");
      text.setAttribute("font-size", fontSize.toString());
      svg.appendChild(text);
      return text;
    };

    const percentText = createText(-40, 24);
    const priceText = createText(0, 20);
    const timeText = createText(40, 18);

    let animationFrameId: number;
    const update = () => {
      const now = Date.now();
      const remainingMs = Math.max(0, endTs - now);
      const ratio = remainingMs / DEFAULT_MS;
      const activeIndex = Math.floor(ratio * 99);

      tickCircles.forEach((circle, index) => {
        circle.setAttribute("fill", index === activeIndex ? "#FF0000" : "#ccc");
      });

      percentText.textContent = Math.floor(ratio * 100) + "%";
      const currentPrice = minPrice + (startingPrice - minPrice) * ratio;
      priceText.textContent = "€" + currentPrice.toFixed(2);
      timeText.textContent = formatMs(remainingMs);

      animationFrameId = requestAnimationFrame(update);
    };
    update();

    return () => cancelAnimationFrame(animationFrameId);
  }, [endTs]);

  const handleReset = () => {
    const newEnd = Date.now() + DEFAULT_MS;
    setEndTs(newEnd);
    writeEndToStorage(newEnd);
  };

  const handleStop = () => setEndTs(null);

  return (
    <>
    <div className="veiling-page">
            <Topbar
                currentPage="Veiling"
                useSideBar={false}
            />
    
      <div className="clock-container">
        <svg ref={svgRef} width="700" height="700"></svg>
      </div>

      <div className="gallery">
        <PhotoRow photos={PHOTOS} />
      </div>


      <div className="sidebar">
        <div className="sidebar-top">
          <p>Naam: Zonnebloemen</p>
          <p>
            Beschrijving: Dit is een voorbeeld om een idee te geven hoe deze pagina er uit zou zien.
            De zonnebloem is een tot 3 meter hoge, eenjarige plant uit de composietenfamilie.
            De zonnebloem kan gezaaid worden van april tot half juni.
          </p>
        </div>
        <div className="sidebar-bottom">
          <button onClick={handleStop}>Koop</button>
          <button onClick={handleReset}>Reset</button>
        </div>
      </div>
    </div>
    </>
  );
}
