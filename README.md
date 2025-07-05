# 🧠 Caching Proxy Server CLI (C#)

A simple command-line caching proxy server built with **.NET Core**.  
It forwards HTTP requests to an origin server and caches the responses in memory.  
If the same request is made again, the cached response is returned instead of hitting the origin.

---

## 🚀 Features

- ✅ Forward requests to an origin server
- ✅ Cache responses in memory
- ✅ Serve cached responses for repeated requests
- ✅ Add `X-Cache: HIT` or `X-Cache: MISS` headers
- ✅ Clear cache using a CLI flag
- ✅ Lightweight and simple
