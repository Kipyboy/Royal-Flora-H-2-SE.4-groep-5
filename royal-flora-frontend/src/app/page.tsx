// page.tsx
// Startpagina (Home) van de applicatie.
// Deze component toont twee navigatieknoppen: Inloggen en Registreren.

// 'use client' geeft aan dat dit een client-side component is in Next.js.
// Dit is nodig wanneer je client-side gedrag of hooks wilt gebruiken.
'use client';

// Importeer de Link-component van Next.js voor client-side navigatie.
import Link from 'next/link';

// Home-component (default export)
// - Renderen: een gecentreerde container met titel en twee 'knoppen' (links).
// - Styling: tijdelijk inline gebruikt voor eenvoudige layout; kan later naar CSS verplaatst worden.
export default function Home() {
  return (
    <div style={{ 
      display: 'flex', 
      flexDirection: 'column', 
      alignItems: 'center', 
      justifyContent: 'center', 
      minHeight: '100vh',
      gap: '20px',
      backgroundColor: '#172D13',
      color: 'white'
    }}>
      {/* Titel van de applicatie */}
      <h1>Royal Flora Holland</h1>

      {/* Container voor de navigatieknoppen */}
      <div style={{ display: 'flex', gap: '20px' }}>
        {/* Link naar de inlogpagina. Gestyled als knop via inline styles. */}
        <Link 
          href="/login" 
          style={{ 
            padding: '10px 20px', 
            backgroundColor: '#6BB77B', 
            color: 'white', 
            textDecoration: 'none',
            borderRadius: '5px'
          }}
        >
          Inloggen
        </Link>

        {/* Link naar de registratiepagina. */}
        <Link 
          href="/registreren" 
          style={{ 
            padding: '10px 20px', 
            backgroundColor: '#4A9D5F', 
            color: 'white', 
            textDecoration: 'none',
            borderRadius: '5px'
          }}
        >
          Registreren
        </Link>
      </div>
    </div>
  );
}