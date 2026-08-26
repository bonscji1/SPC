import * as esbuild from 'esbuild';
import { mkdir } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const outDir = path.join(root, 'src/SPC.Web/wwwroot/js/instruction-editor');

await mkdir(outDir, { recursive: true });

await esbuild.build({
  entryPoints: [path.join(outDir, 'instruction-editor.js')],
  bundle: true,
  outfile: path.join(outDir, 'instruction-editor.bundle.js'),
  format: 'iife',
  platform: 'browser',
  target: ['es2020'],
  sourcemap: true,
  logLevel: 'info',
});

console.log('Built instruction-editor.bundle.js');
