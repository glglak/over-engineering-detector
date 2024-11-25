/** @type {import('next').NextConfig} */
module.exports = {
  output: 'export',
  images: {
    unoptimized: true, // GitHub Pages does not support image optimization
  },
};