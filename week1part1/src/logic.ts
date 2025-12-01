import { readFile as fsReadFile } from 'fs/promises';

export async function readDataset(filePath: string): Promise<number[]> {
  const content = await fsReadFile(filePath, 'utf-8');
  const lines = content.trim().split('\n');
  
  const numbers: number[] = [];
  
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim();
    if (line === '') continue;
    
    const num = Number(line);
    
    if (!Number.isInteger(num)) {
      throw new Error(`Line ${i + 1} is not a valid integer: "${line}"`);
    }
    
    numbers.push(num);
  }
  
  return numbers;
}

export function sumOutliers(dataset: number[]): number {
  if (dataset.length === 0) {
    return 0;
  }
  
  const mean = dataset.reduce((sum, num) => sum + num, 0) / dataset.length;
  
  // standard deviation
  const variance = dataset.reduce((sum, num) => sum + Math.pow(num - mean, 2), 0) / dataset.length;
  const stdDev = Math.sqrt(variance);
  
  // Sum numbers that are more than 2 standard deviations away from the mean
  const threshold = 2 * stdDev;
  return dataset
    .filter(num => Math.abs(num - mean) > threshold)
    .reduce((sum, num) => sum + num, 0);
}

