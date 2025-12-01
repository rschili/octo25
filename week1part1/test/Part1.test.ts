import path from 'path';
import { test, expect } from 'vitest';
import { readDataset, sumOutliers } from '../src/logic';

test('processes test input file', async () => {
  const filePath = path.resolve(__dirname, 'testdata.txt');
  const numbers = await readDataset(filePath);
  expect(numbers.length).toEqual(20);
  const result = sumOutliers(numbers);
  expect(result).toEqual(2025); // Expected test dataset result
});
