const http = require('http');
const url = require('url');

const server = http.createServer((req, res) => {
    const parsedUrl = url.parse(req.url, true);
    const path = parsedUrl.pathname;
    const query = parsedUrl.query;

    // 设置 CORS 头
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, PATCH');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, User-Agent, Accept');
    
    // 处理 OPTIONS 请求
    if (req.method === 'OPTIONS') {
        res.writeHead(200);
        res.end();
        return;
    }

    // 设置一个测试 Cookie
    res.setHeader('Set-Cookie', ['test-cookie=test-value; Path=/']);

    console.log(`${req.method} ${req.url}`);
    console.log('Headers:', req.headers);
    console.log('Query:', query);

    // 构建响应
    const response = {
        method: req.method,
        path: path,
        query: query,
        headers: req.headers,
        timestamp: new Date().toISOString(),
        message: `Hello from API test server! Path: ${path}`
    };

    if (req.method === 'POST' || req.method === 'PUT' || req.method === 'PATCH') {
        let body = '';
        req.on('data', chunk => {
            body += chunk.toString();
        });
        req.on('end', () => {
            response.body = body;
            res.writeHead(200, { 'Content-Type': 'application/json' });
            res.end(JSON.stringify(response, null, 2));
        });
    } else {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify(response, null, 2));
    }
});

const PORT = 3001;
server.listen(PORT, () => {
    console.log(`Test API server running on http://localhost:${PORT}`);
    console.log('Try accessing:');
    console.log(`  http://localhost:${PORT}/api/test?type=1&number=2&key=3`);
});
