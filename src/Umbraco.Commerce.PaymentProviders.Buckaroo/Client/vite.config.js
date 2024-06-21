import { defineConfig } from 'vite';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
    build: {
        lib: {
            entry: 'src/index.ts',
            formats: ['es'],
        },
        outDir: '../wwwroot/dist/',
        emptyOutDir: true,
        sourcemap: true,
        build: {
            rollupOptions: {
                external: [/^@umbraco/],
                output: {
                    chunkFileNames: '[name].js',
                },
            },
        },
    },
    plugins: [tsconfigPaths()],
});
