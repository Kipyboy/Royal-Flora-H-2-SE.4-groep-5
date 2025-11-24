"use client";
import React from "react";
import '../../styles/veiling-sidebar.css';

interface SidebarProps {
  onReset: () => void;
  onStop: () => void;
}

export default function Sidebar({ onReset, onStop }: SidebarProps) {
  return (
    <div className="sidebar">
      <div className="sidebar-top">
        <p>Naam: Zonnebloemen</p>
        <br></br>
        <p>
          Beschrijving: Dit is een voorbeeld om een idee te geven hoe deze pagina er uit zou zien.
          De zonnebloem is een tot 3 meter hoge, eenjarige plant uit de composietenfamilie.
          De zonnebloem kan gezaaid worden van april tot half juni.
        </p>
      </div>
      <div className="sidebar-bottom">
        <button onClick={onStop}>Koop</button>
        <button onClick={onReset}>Reset</button>
      </div>
    </div>
  );
}