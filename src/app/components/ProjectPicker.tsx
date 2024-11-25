'use client';
import React from 'react';
import { Button } from './ui/button';

import { FileCode } from 'lucide-react';

export function ProjectPicker({ onSelect }: { onSelect: (path: string) => void }) {
  const handleFileSelect = async () => {
    try {
      // @ts-expect-error: File System Access API is experimental
      const dirHandle = await window.showDirectoryPicker();
      onSelect(dirHandle.name);
    } catch (err) {
      console.error('Error:', err);
    }
  };

  return (
    <Button onClick={handleFileSelect} className="flex items-center gap-2">
      <FileCode className="h-4 w-4" />
      Select Project
    </Button>
  );
}