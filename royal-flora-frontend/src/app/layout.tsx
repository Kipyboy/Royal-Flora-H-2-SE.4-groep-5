// layout.tsx
// Hoofdlayout voor de Next.js-applicatie.
// Dit bestand stelt globale fonts en root-stijlen in, definieert pagina-metadata
// en rendert de HTML-structuur waarin de rest van de app wordt geplaatst.

import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "../styles/Root.css"; // Keep only global/root styles here

// Laad Google-fonts via Next.js' ingebouwde font-optimalisatie.
// We zetten de fonts als CSS-variabelen zodat ze eenvoudig op <body> toegepast kunnen worden.
const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

// Metadata die Next.js kan gebruiken voor de pagina (bijv. <title>, description etc.).
export const metadata: Metadata = {
  title: "Royal Flora Holland",
  description: "Bloemen veiling platform",
};

// RootLayout: de top-level lay-outcomponent die door Next.js gebruikt wordt.
// Props:
// - children: de nested pagina/componenten die binnen deze layout gerenderd worden.
// We zetten expliciet `lang="nl"` en voegen de font-variabelen toe aan de body-class
// zodat de hele site gebruik maakt van de geladen fonts.
export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="nl">
      <body className={`${geistSans.variable} ${geistMono.variable}`}>
        {children}
      </body>
    </html>
  );
}