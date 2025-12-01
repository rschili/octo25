#!/usr/bin/env node

import chalk from "chalk";
import path from 'path';
import { fileURLToPath } from 'url';
import { readDataset, sumOutliers } from "./logic";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const filePath = path.resolve(__dirname, 'data.txt');
const numbers = await readDataset(filePath);
if(numbers.length !== 1000000) {
    console.error(chalk.red(`Expected 1,000,000 numbers but got ${numbers.length}`));
    process.exit(1);
}

const result = sumOutliers(numbers);
console.log(`Sum of outliers in actual data: ${chalk.green(result)}`);
process.exit(0);
