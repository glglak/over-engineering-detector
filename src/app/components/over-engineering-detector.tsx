'use client';

import React, { useState } from "react";
import { 
  FileCode, 
  FolderTree, 
  Layers, 
  Code, 
  FileSearch, 
  AlertCircle, 
  Settings,
  Trophy,
  AlertTriangle,
  Skull,
  CheckCircle ,
  Brain 
} from 'lucide-react';

type ProjectMetrics = {
  TotalFiles: number;
  TotalDirectories: number;
  AverageNestingLevels: number;
  DeepestDirectory: string;
  FileTypeDistribution: Record<string, number>;
  Services: number;
  Projects: number;
  DesignPatterns: string[];
  ArchitectureLayers: number;
};
type MetricCardProps = {
  title: string; // The title must be a string
  value: number | string; // Value can be a number or string
  icon: JSX.Element; // Icon must be a React JSX element
  alert?: boolean; // Alert is optional and defaults to false
};

const MetricCard: React.FC<MetricCardProps> = ({ title, value, icon, alert = false }) => (
  <div className={`p-4 ${alert ? 'bg-red-50' : 'bg-gray-50'} rounded-lg transition-colors`}>
    <div className="flex items-center gap-2 mb-2">
      {icon}
      <span className="text-sm font-medium text-gray-600">{title}</span>
    </div>
    <div className={`text-2xl font-bold ${alert ? 'text-red-600' : 'text-gray-900'}`}>{value}</div>
  </div>
);

const ProjectAnalyzer = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [metrics, setMetrics] = useState<ProjectMetrics | null>(null);
  const [directoryStructure, setDirectoryStructure] = useState<Record<string, unknown> | null>(null);
  const [roastingMessage, setRoastingMessage] = useState<string | null>(null);
  const [roastingTip, setRoastingTip] = useState<string | null>(null);
  const [folderName, setFolderName] = useState<string | null>(null);
  const [isCurrentlyScanning, setIsCurrentlyScanning] = useState(false);

  // Your existing directory scanning functions here
  const scanDirectory = async (directoryHandle: FileSystemDirectoryHandle) => {
    const structure: Record<string, unknown> = {};
    const entries = await directoryHandle.entries();
    const promises = [];

    for await (const [name, handle] of entries) {
      if (handle.kind === "directory" &&
        name !== "test" &&
        name !== "bin" &&
        name !== ".git" &&
        name !== ".vs" &&
        name !== "obj" &&
        name !== ".gitIgnore") {
        promises.push(
          scanDirectory(handle as FileSystemDirectoryHandle).then((subStructure) => {
            structure[name] = subStructure;
          })
        );
      } else if (handle.kind === "file") {
        structure[name] = "file";
      }
    }

    await Promise.all(promises);
    return structure;
  };

  const handleDirectorySelect = async () => {
    try {
      // @ts-expect-error: File System Access API is experimental
      const directoryHandle: FileSystemDirectoryHandle = await window.showDirectoryPicker();
      setIsCurrentlyScanning(true);
      const projectStructure = await scanDirectory(directoryHandle);
      setDirectoryStructure(projectStructure);
      setIsCurrentlyScanning(false);
      setFolderName(directoryHandle.name);
    } catch (error) {
      console.error("Error selecting directory:", error);
    }
  };

  const sendStructureToServer = async (structure: Record<string, unknown>) => {
    if (!structure) {
      alert("No directory structure available for analysis.");
      return;
    }
  
    setIsLoading(true);
  
    try {
      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/analyzer/analyze`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Structure: structure }),
      });
  
      if (response.ok) {
        const rawData = await response.text();
        const data = JSON.parse(JSON.parse(rawData));
        setRoastingMessage(data.roastingMessage || "No feedback provided.");
        setRoastingTip(data.roastingTip || null);

        setMetrics(data.metrics || null);
      } else {
        const errorText = await response.text();
        alert(`Server error: ${errorText}`);
      }
    } catch (err) {
      console.error("Failed to connect to the server:", err);
      alert("Failed to connect to the server. Check your connection.");
    } finally {
      setIsLoading(false);
    }
  };

  const clearDirectory = () => {
    setDirectoryStructure(null);
    setMetrics(null);
    setRoastingMessage(null);
    setRoastingTip(null);
    setFolderName(null);
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <FolderTree className="h-8 w-8 text-blue-500" />
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Project Structure Analyzer</h1>
                <p className="text-gray-500">Analyze and improve your project architecture</p>
              </div>
            </div>
            <div className="space-x-4">
              <div>
                <button
                onClick={handleDirectorySelect}
                className="inline-flex items-center px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors"
                disabled={isLoading}
                >
                <FileSearch className="h-4 w-4 mr-2" />
                Select Directory
                </button>
                <button
                onClick={() => directoryStructure && sendStructureToServer(directoryStructure)}
                className="inline-flex items-center px-4 py-2 bg-green-500 text-white rounded-md hover:bg-green-600 transition-colors"
                disabled={isLoading || !directoryStructure}
                >
                <Code className="h-4 w-4 mr-2" />
                Analyze
                </button>
              </div>
                {directoryStructure && (
                <>
                  <div className="mt-4 text-gray-600 text-left">
                  <strong>Selected Directory:</strong> {folderName}
                  </div>
                  <button
                  type="button"
                  onClick={clearDirectory}
                  className="inline-flex items-center px-4 py-2 bg-red-500 text-white rounded-md hover:bg-red-600 transition-colors"
                  disabled={isCurrentlyScanning}
                  >
                  <AlertCircle className="h-4 w-4 mr-2" />
                  Clear
                  </button>
                </>
                )}
                {isCurrentlyScanning && (
                <div className="flex items-center justify-center p-8 bg-white rounded-lg shadow-sm mb-6">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mr-3"></div>
                  <p className="text-gray-600">Scanning directory...</p>
                </div>
                )}
            </div>
          </div>

        </div>

        {isLoading && (
          <div className="flex items-center justify-center p-8 bg-white rounded-lg shadow-sm mb-6">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mr-3"></div>
            <p className="text-gray-600">Analyzing your project structure...</p>
          </div>
        )}

        {/* Results Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {roastingMessage && metrics && (
            <div className="lg:col-span-3 space-y-6">
                  <div className="bg-indigo-900 text-white rounded-lg shadow-sm p-6">
      <div className="flex items-center gap-3">
        <Brain className="h-8 w-8 text-indigo-300" />
        <div>
          <div className="text-indigo-300 text-sm font-medium">ARCHITECT&apos;S Roast Line</div>
          <div className="text-xl font-bold italic">&quot;{roastingTip}&quot;</div>
        </div>
      </div>
    </div>

              {/* Shame Badge */}
              <div className={`bg-white rounded-lg shadow-sm p-6 ${
                metrics.AverageNestingLevels > 5 ? 'border-l-4 border-red-500' :
                metrics.AverageNestingLevels > 3 ? 'border-l-4 border-orange-500' :
                'border-l-4 border-green-500'
              }`}>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    {metrics.AverageNestingLevels > 5 ? (
                      <Skull className="h-8 w-8 text-red-500" />
                    ) : metrics.AverageNestingLevels > 3 ? (
                      <AlertTriangle className="h-8 w-8 text-orange-500" />
                    ) : (
                      <Trophy className="h-8 w-8 text-green-500" />
                    )}
                    <div>
                      <h3 className="text-xl font-bold">
                        {metrics.AverageNestingLevels > 5 ? 'Architecture Astronaut' :
                         metrics.AverageNestingLevels > 3 ? 'Complexity Creator' :
                         'Clean Code Champion'}
                      </h3>
                      <p className="text-gray-500">
                        {metrics.AverageNestingLevels > 5 ? 'Cairo, we have an over-engineering problem!' :
                         metrics.AverageNestingLevels > 3 ? 'Some complexity detected, might need simplification' :
                         'Nice job keeping it clean and simple!'}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <div className="text-sm text-gray-500 mb-1">Complexity Score</div>
                    <div className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${
                      metrics.AverageNestingLevels > 5 ? 'bg-red-100 text-red-700' :
                      metrics.AverageNestingLevels > 3 ? 'bg-orange-100 text-orange-700' :
                      'bg-green-100 text-green-700'
                    }`}>
                      {(metrics.AverageNestingLevels * 20) + (metrics.Services * 10) + (metrics.ArchitectureLayers * 15)}
                    </div>
                  </div>
                </div>
              </div>

              {/* Enhanced Roasting Message */}
              <div className="bg-white rounded-lg shadow-sm p-6">
                <div className="flex items-center gap-3 mb-4">
                  <AlertCircle className="h-6 w-6 text-orange-500" />
                  <h2 className="text-xl font-bold">Architecture Analysis</h2>
                </div>
                <div className="space-y-4">
    {roastingMessage.split(/\d+\.\s*/).filter(Boolean).map((point, index) => {
      const isPositive = point.toLowerCase().includes('well-defined') || 
                        point.toLowerCase().includes('consistent') ||
                        point.toLowerCase().includes('clean') ||
                        point.toLowerCase().includes('good') ||
                        point.toLowerCase().includes('adherence');
      
      return (
        <div 
          key={index} 
          className="flex items-start gap-3 p-3 bg-orange-50 rounded-lg transition-all hover:bg-orange-100"
        >
          {isPositive ? (
            <CheckCircle className="h-5 w-5 text-green-500 mt-1" />
          ) : (
            <AlertTriangle className="h-5 w-5 text-orange-500 mt-1" />
          )}
          <p className="text-gray-700 flex-1">
            {index + 1}. {point.trim()}
          </p>
        </div>
      );
    })}
  </div>
              </div>
            </div>
          )}

{metrics && (
            <>
              {/* Key Metrics */}
              <div className="bg-white rounded-lg shadow-sm p-6">
                <div className="flex items-center gap-2 mb-4">
                  <Settings className="h-5 w-5 text-blue-500" />
                  <h2 className="text-xl font-semibold">Key Metrics</h2>
                </div>
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <MetricCard
                      title="Total Files"
                      value={metrics.TotalFiles}
                      icon={<FileCode className="h-4 w-4 text-blue-500" />}
                    />
                    <MetricCard
                      title="Total Directories"
                      value={metrics.TotalDirectories}
                      icon={<FolderTree className="h-4 w-4 text-green-500" />}
                    />
                    <MetricCard
                      title="Services"
                      value={metrics.Services}
                      icon={<Settings className="h-4 w-4 text-purple-500" />}
                    />
                    <MetricCard
                      title="Projects"
                      value={metrics.Projects}
                      icon={<Code className="h-4 w-4 text-orange-500" />}
                    />
                  </div>
                  <div className="pt-4 border-t">
                    <h3 className="text-sm font-medium text-gray-500 mb-2">Deepest Directory</h3>
                    <p className="text-sm bg-gray-50 p-2 rounded break-all">{metrics.DeepestDirectory}</p>
                  </div>
                </div>
              </div>

              {/* File Distribution */}
              <div className="bg-white rounded-lg shadow-sm p-6">
                <div className="flex items-center gap-2 mb-4">
                  <FileCode className="h-5 w-5 text-green-500" />
                  <h2 className="text-xl font-semibold">File Distribution</h2>
                </div>
                <div className="space-y-3">
                  {Object.entries(metrics.FileTypeDistribution).map(([type, count]) => (
                    <div key={type} className="flex justify-between items-center">
                      <span className="text-sm font-medium text-gray-600">{type}</span>
                      <div className="flex items-center">
                        <div className="h-2 bg-blue-100 rounded-full w-32 mr-3">
                          <div
                            className="h-2 bg-blue-500 rounded-full"
                            style={{
                              width: `${(count / Math.max(...Object.values(metrics.FileTypeDistribution))) * 100}%`
                            }}
                          />
                        </div>
                        <span className="text-sm text-gray-500">{count}</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Design Patterns */}
              <div className="bg-white rounded-lg shadow-sm p-6">
                <div className="flex items-center gap-2 mb-4">
                  <Layers className="h-5 w-5 text-purple-500" />
                  <h2 className="text-xl font-semibold">Design Patterns</h2>
                </div>
                <div className="flex flex-wrap gap-2">
                  {metrics.DesignPatterns.map((pattern, index) => (
                    <span
                      key={index}
                      className="px-3 py-1 bg-purple-50 text-purple-700 rounded-full text-sm"
                    >
                      {pattern}
                    </span>
                  ))}
                </div>
              </div>
            </>
          )}
        
        </div>
      </div>
    </div>
  );
};

export default ProjectAnalyzer;