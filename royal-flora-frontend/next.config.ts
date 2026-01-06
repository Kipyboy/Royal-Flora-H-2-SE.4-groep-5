import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  
  // Proxy /api and /images requests to the backend
  async rewrites() {
    // Use BACKEND_URL for server-side rewrites (Docker internal network)
    // Falls back to localhost for local development
    const backendUrl = process.env.BACKEND_URL || 'http://localhost:5156';
    return [
      {
        source: '/api/:path*',
        destination: `${backendUrl}/api/:path*`,
      },
      {
        source: '/images/:path*',
        destination: `${backendUrl}/images/:path*`,
      },
    ];
  },
};

export default nextConfig;
