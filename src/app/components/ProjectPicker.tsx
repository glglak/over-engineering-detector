'use client';
import React from 'react';
import { Button } from './ui/button';
import { FileCode } from 'lucide-react';

export function ProjectPicker({ onSelect }: { onSelect: (path: string) => void }) {
  const handleFileSelect = async () => {
    try {
      // Configure the file picker to accept .sln files
      const fileHandles = await window.showOpenFilePicker({
        types: [{
          description: 'Solution Files',
          accept: { 'application/vnd.microsoft.portable-executable': ['.sln'] }
        }],
        multiple: false  // Set to true if you want to allow selecting multiple files
      });

      if (fileHandles.length > 0) {
        const fileHandle = fileHandles[0];
        onSelect(fileHandle.name);  // You may need to handle the file or read its contents based on your app's logic
      }
    } catch (err) {
      console.error('Error:', err);
      alert('Failed to open file picker. Please try again.');
    }
  };

  return (
    <Button onClick={handleFileSelect} className="flex items-center gap-2">
      <FileCode className="h-4 w-4" />
      Select Solution File
    </Button>
  );
}
