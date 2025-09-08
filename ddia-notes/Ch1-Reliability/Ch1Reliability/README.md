# Chapter 1 – Reliability (Hands-on)

This demo brings **Designing Data-Intensive Applications, Chapter 1 (Reliability)** to life with runnable .NET code.

Key idea from the book:  
> A fault doesn’t have to become a failure if we design systems with fallbacks.

---

## 🔑 What’s inside

- **Flaky API** → Minimal API that fails the first *N* requests before succeeding  
- **Retry Client** → Console app with Polly retry + exponential backoff (*fault ≠ failure*)  
- **Circuit-Breaker Client** → Console app with Polly circuit breaker (*containment & fast-fail*)  

---

## 📂 Structure

- **Ch1-Reliability/**
  - **FlakyApi/** → minimal API, simulates transient faults  
  - **RetryClient/** → retries with backoff (*fault ≠ failure*)  
  - **CircuitClient/** → circuit breaker (*isolation & containment*)  

---

## ▶️ Run the demo

### 1) Start the API

```bash
dotnet run --project FlakyApi --urls http://localhost:5080
```

Or make it nastier:

```bash
FAIL_FIRST=4 dotnet run --project FlakyApi --urls http://localhost:5080
```

---

### 2) Run the Retry Client

```bash
dotnet run --project RetryClient
```

---

### 3) Run the Circuit-Breaker Client

```bash
dotnet run --project CircuitClient
```

---

## 🎯 Concepts demonstrated

- **Fault vs Failure vs Fallback**
  - Fault → transient 503 or dropped connection  
  - Failure → what the user would see without retries  
  - Fallback → retry policy, circuit breaker, cache, etc.  

- **Retries with backoff** → convert many faults into success  
- **Circuit breaker** → prevent cascading failures and isolate unhealthy dependencies  

> **Takeaway:** A reliable system is one where faults don’t always lead to failures.

---

## 📚 References

- Martin Kleppmann, *Designing Data-Intensive Applications* — Chapter 1: Reliability  
- [Polly GitHub](https://github.com/App-vNext/Polly) – resilience & transient-fault-handling library for .NET
