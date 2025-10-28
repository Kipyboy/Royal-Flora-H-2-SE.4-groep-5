'use client';

import Link from 'next/link';

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
      <h1>Royal Flora Holland</h1>
      <div style={{ display: 'flex', gap: '20px' }}>
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