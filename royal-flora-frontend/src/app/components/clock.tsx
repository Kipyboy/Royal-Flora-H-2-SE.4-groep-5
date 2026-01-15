"use client";
import React, { useEffect, useRef, useState } from "react";
import "../../styles/clock.css";
import { API_BASE_URL } from "../config/api";

// Formatteer milliseconden naar mm:ss.mmm
function formatMs(ms: number) {
  if (ms <= 0) return "00:00.000";
  const totalSec = Math.floor(ms / 1000);
  const min = Math.floor(totalSec / 60);
  const sec = totalSec % 60;
  const millis = ms % 1000;
  return `${String(min).padStart(2, "0")}:${String(sec).padStart(2,"0")}.${String(millis).padStart(3, "0")}`;
}

// Klok component: toont een grafische klok (SVG) en bepaalt actuele prijs op basis van
// resterende tijd tussen een startprijs en minimale prijs. Roept optioneel `onPriceChange`
// aan met de actuele prijs en `onFinished` zodra de klok op 0 raakt.
interface ClockProps {
  elapsed: number;
  startTime: number | null;
  durationMs: number;
  onPriceChange?: (price: number) => void;
  locationName?: string;
  onFinished?: () => void;
}

interface KlokDTO {
  minimumPrijs: number;
  startPrijs: number;
}

export default function Clock({ elapsed, startTime, durationMs, onPriceChange, locationName, onFinished }: ClockProps) { 
  const svgRef = useRef<SVGSVGElement | null>(null);
  const [minPrice, setMinPrice] = useState<number | null>(null);
  const [startPrijs, setStartPrijs] = useState<number | null>(null);
  const finishedRef = useRef(false);

  // Fetch klok-informatie (minimumPrijs en startPrijs) voor de gegeven locatie.
  // Wanneer geen actieve veiling is, zetten we minPrice op -1 zodat de UI een melding toont.
  useEffect(() => {
    if (!locationName) return;

    fetch(`${API_BASE_URL}/api/Products/Klok?locatie=${encodeURIComponent(locationName)}`)
      .then(async (res) => {
        if (res.status === 404) { setMinPrice(-1); return null; }
        if (!res.ok) { setMinPrice(-1); return null; }
        return res.text();
      })
      .then((text) => {
        if (!text) return;
        const data: KlokDTO = JSON.parse(text);
        setMinPrice(data.minimumPrijs);
        setStartPrijs(data.startPrijs);
      })
      .catch(() => setMinPrice(-1));
  }, [locationName]);

  // Bouw het SVG-klokje en update de visuals gebaseerd op de resterende tijd.
  // We creëren ticks, tekstvelden en een `update` functie die de kleur/tekst bijwerkt.
  useEffect(() => {
    if (!svgRef.current || minPrice === -1) return;

    const svg = svgRef.current;
    svg.innerHTML = "";

    const radius = 200;
    const center = radius + 50;

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

    const update = () => {
      if (startTime == null) return;

      const remainingMs = Math.max(0, durationMs - elapsed);
      const ratio = Math.min(1, Math.max(0, remainingMs / durationMs));
      const activeIndex = Math.floor(ratio * 99);

      // Wanneer de klok afgelopen is, roep onFinished aan (éénmalig)
      if (remainingMs <= 0 && !finishedRef.current) {
        finishedRef.current = true;
        onFinished?.();
      }

      tickCircles.forEach((circle, index) => {
        // Active tick in het rood tonen
        circle.setAttribute("fill", index === activeIndex ? "#FF0000" : "#ccc");
      });

      percentText.textContent = `${Math.floor(ratio * 100)}%`;

      const baseMin = minPrice ?? 0;
      const baseStart = startPrijs ?? baseMin;
      const currentPrice = baseMin + (baseStart - baseMin) * ratio;

      // Toon actuele prijs en informeer parent via callback
      priceText.textContent = "€" + currentPrice.toFixed(2);
      onPriceChange?.(Number(currentPrice.toFixed(2)));

      timeText.textContent = formatMs(remainingMs);
    };

    update();
  }, [elapsed, startTime, durationMs, minPrice, startPrijs, onPriceChange, onFinished]);

  if (minPrice === -1) {
    return <div className="notice">Geen veiling gaande op deze locatie</div>;
  }

  return <svg ref={svgRef} viewBox="0 0 500 500"></svg>;
}
