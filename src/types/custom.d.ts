// custom.d.ts
declare global {
    namespace JSX {
      interface IntrinsicElements {
        input: React.DetailedHTMLProps<React.InputHTMLAttributes<HTMLInputElement>, HTMLInputElement> & {
          webkitdirectory?: boolean;
        };
      }
    }
  }
  
  // Include this line if "dom" is not already included in your tsconfig lib array
  /// <reference lib="dom" />
  