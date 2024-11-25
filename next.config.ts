/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'export',
  images: {
    unoptimized: true, // Required for GitHub Pages or any static export without image optimization
  },
};

module.exports = nextConfig;