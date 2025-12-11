// API Configuration
// Uses environment variable NEXT_PUBLIC_API_URL, defaults to localhost for development

export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5156';

// Helper function to build API URLs
export function apiUrl(path: string): string {
  // Ensure path starts with /
  const cleanPath = path.startsWith('/') ? path : `/${path}`;
  return `${API_BASE_URL}${cleanPath}`;
}

// Helper for image URLs
export function imageUrl(imagePath: string): string {
  return `${API_BASE_URL}/images/${imagePath}`;
}
