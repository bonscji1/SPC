FROM node:22-alpine AS js
WORKDIR /web
COPY package.json package-lock.json ./
RUN npm ci
COPY scripts ./scripts
COPY src/SPC.Web/wwwroot/js/instruction-editor ./src/SPC.Web/wwwroot/js/instruction-editor
RUN npm run build:instruction-editor

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props ./
COPY src/SPC.Core ./src/SPC.Core
COPY src/SPC.Web ./src/SPC.Web
COPY --from=js /web/src/SPC.Web/wwwroot/js/instruction-editor/instruction-editor.bundle.js src/SPC.Web/wwwroot/js/instruction-editor/
COPY --from=js /web/src/SPC.Web/wwwroot/js/instruction-editor/instruction-editor.bundle.js.map src/SPC.Web/wwwroot/js/instruction-editor/
RUN dotnet publish src/SPC.Web/SPC.Web.csproj -c Release -o /app/publish

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
