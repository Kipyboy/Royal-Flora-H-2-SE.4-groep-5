"use client";
import React, { useEffect, useRef, useState } from "react";
import '../../styles/clock.css';

function formatMs(ms: number) {
  if (ms <= 0) return "00:00.000";
  const totalSec = Math.floor(ms / 1000);
  const min = Math.floor(totalSec / 60);
  const sec = totalSec % 60;
  const millis = ms % 1000;
  return `${String(min).padStart(2, "0")}:${String(sec).padStart(2, "0")}.${String(millis).padStart(3, "0")}`;
}

interface ClockProps {
  endTs: number | null;
  durationMs: number;
}

interface KlokDTO {
  minimumPrijs: number;
}

export default function Clock({ endTs, durationMs }: ClockProps) {
  const svgRef = useRef<SVGSVGElement | null>(null);
  const [minPrice, setMinPrice] = useState<number | null>(null);

  useEffect(() => {
    fetch("http://localhost:5156/api/Products/Klok")
      .then(res => res.json())
      .then((data: KlokDTO[] | any) => {
        if (Array.isArray(data) && data.length > 0) {
          const prices = data.map(p => Number(p.minimumPrijs ?? p.MinimumPrijs ?? 0)).filter(p => !Number.isNaN(p));
          const min = prices.length ? Math.min(...prices) : 0;
          setMinPrice(min);
        } else {
          setMinPrice(0);
        }
      })
      .catch(err => {
        console.error("Error fetching product:", err);
        setMinPrice(0);
      });
  }, []);

  useEffect(() => {
    if (!svgRef.current || !endTs || minPrice === null) return;
    const svg = svgRef.current;
    svg.innerHTML = "";

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

      tickCircles.forEach((circle, index) => {
        circle.setAttribute("fill", index === activeIndex ? "#FF0000" : "#ccc");
      });

      percentText.textContent = `${Math.floor(ratio * 100)}%`;
      const currentPrice = minPrice + (startingPrice - minPrice) * ratio;
      priceText.textContent = "€" + currentPrice.toFixed(2);
      timeText.textContent = formatMs(remainingMs);

      animationFrameId = requestAnimationFrame(update);
    };
    update();

    return () => cancelAnimationFrame(animationFrameId);
  }, [endTs, durationMs, minPrice]);

  return (<svg ref={svgRef} viewBox="0 0 500 500"></svg>);
}