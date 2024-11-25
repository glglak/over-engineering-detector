export {};

declare global {
  interface Window {
    showDirectoryPicker?: () => Promise<FileSystemDirectoryHandle>;
    showOpenFilePicker?: (options?: {
        types?: {
          description?: string;
          accept?: Record<string, string[]>;
        }[];
        excludeAcceptAllOption?: boolean;
        multiple?: boolean;
      }) => Promise<FileSystemFileHandle[]>;
  }
  
}
