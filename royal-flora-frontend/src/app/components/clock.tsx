"use client";
import React, { useEffect, useRef, useState } from "react";
import "../../styles/clock.css";
import { API_BASE_URL } from "../config/api";

function formatMs(ms: number) {
  if (ms <= 0) return "00:00.000";
  const totalSec = Math.floor(ms / 1000);
  const min = Math.floor(totalSec / 60);
  const sec = totalSec % 60;
  const millis = ms % 1000;
  return `${String(min).padStart(2, "0")}:${String(sec).padStart(2,"0")}.${String(millis).padStart(3, "0")}`;
}

interface ClockProps {
  endTs: number | null;
  durationMs: number;
  onPriceChange?: (price: number) => void;
  locationName?: string;
  onFinished?: () => void;
}

interface KlokDTO {
  minimumPrijs: number;
  locatie?: string;
  status?: number;
}

export default function Clock({endTs, durationMs, onPriceChange, locationName, onFinished}: ClockProps) {
  const svgRef = useRef<SVGSVGElement | null>(null);
  const [minPrice, setMinPrice] = useState<number | null>(null);
  const finishedRef = useRef(false); 

  useEffect(() => {
    if (!locationName) return;

    fetch(
      `${API_BASE_URL}/api/Products/Klok?locatie=${encodeURIComponent(
        locationName
      )}`
    )
      .then(async (res) => {
        if (res.status === 404) {
          setMinPrice(-1);
          return null;
        }
        if (!res.ok) {
          const errorText = await res.text();
          console.error("Error fetching klok data:", res.status, errorText);
          setMinPrice(-1);
          return null;
        }
        return res.text();
      })
      .then((text) => {
        if (!text) return;
        try {
          const data: KlokDTO = JSON.parse(text);
          const price = Number(data.minimumPrijs);
          setMinPrice(Number.isFinite(price) ? price : 0);
        } catch (parseErr) {
          console.error("Failed to parse klok JSON:", parseErr, "Response text:", text);
          setMinPrice(-1);
        }
      })
      .catch((err) => {
        console.error("Error fetching klok data:", err);
        setMinPrice(-1);
      });
  }, [locationName]);

  useEffect(() => {
    if (!svgRef.current || endTs === null || minPrice === null || minPrice === -1)
      return;

    const svg = svgRef.current;
    svg.innerHTML = "";
    finishedRef.current = false;

    const radius = 200;
    const center = radius + 50;
    const startingPrice = minPrice * 10;

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
      const ratio = Math.min(1, Math.max(0, remainingMs / durationMs));
      const activeIndex = Math.floor(ratio * 99);

      if (remainingMs <= 0 && !finishedRef.current) {
        finishedRef.current = true;
        if (onFinished) onFinished();
      }

      tickCircles.forEach((circle, index) => {
        circle.setAttribute("fill", index === activeIndex ? "#FF0000" : "#ccc");
      });

      percentText.textContent = `${Math.floor(ratio * 100)}%`;
      const currentPrice = minPrice + (startingPrice - minPrice) * ratio;
      priceText.textContent = "€" + currentPrice.toFixed(2);

      if (onPriceChange) {
        try {
          onPriceChange(Number(currentPrice.toFixed(2)));
        } catch {}
      }

      timeText.textContent = formatMs(remainingMs);

      animationFrameId = requestAnimationFrame(update);
    };

    update();
    return () => cancelAnimationFrame(animationFrameId);
  }, [endTs, durationMs, minPrice, onPriceChange, onFinished]);

  if (minPrice === -1) {
    return (
    <div className="notice">
       Geen veiling gaande op deze locatie
    </div>
    );
  }

  return <svg ref={svgRef} viewBox="0 0 500 500"></svg>;
}
