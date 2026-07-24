# Senior Azure AI Engineering Residency

**Certification target:** Microsoft Certified: Azure AI Cloud Developer Associate  
**Exam:** AI-200 — Developing AI Cloud Solutions on Azure  
**Duration:** 8 weeks  
**Primary engineering project:** Zapas  
**Blueprint version:** 2.0  
**Official exam outline verified:** July 24, 2026  

---

## 1. Residency Purpose

This residency has two equally explicit outcomes:

1. **Prepare systematically and seriously for Exam AI-200.**
2. **Develop stronger senior-level Azure engineering judgment by applying the measured skills to a real application.**

The certification is not an accidental by-product. Passing AI-200 is a formal program outcome.

The residency is also not a memorization-only exam course. Its main learning vehicle is **Zapas**, an ASP.NET Core application that imports, processes, stores, and analyzes running activity data.

The central operating principle is:

> **Study the official objective, apply it to Zapas or a focused lab, reproduce the skill independently, troubleshoot it, and then practice recognizing it in an exam scenario.**

This creates an **exam-aligned, build-through residency**:

- The official AI-200 skills outline determines what must be covered.
- Zapas provides the practical context for most of the learning.
- Focused labs cover objectives that would be artificial or irresponsible to retain in the permanent Zapas architecture.
- Python exercises provide the language fluency explicitly expected by Microsoft.
- Weekly assessments make certification readiness visible instead of assumed.
- Architecture reviews preserve the senior-engineering objective of the program.

---

## 2. Source of Truth and Exam Alignment

The current Microsoft AI-200 study guide is the source of truth for this residency.

As of July 24, 2026, the official domains are:

| AI-200 domain | Exam weight |
|---|---:|
| Develop containerized solutions on Azure | 20–25% |
| Develop AI solutions by using Azure data management services | 25–30% |
| Connect to and consume Azure services | 20–25% |
| Secure, monitor, and troubleshoot Azure solutions | 20–25% |

Microsoft describes the candidate as a developer who contributes throughout the AI-solution lifecycle, with emphasis on back-end services and components. The expected background includes:

- Azure and third-party SDKs
- Azure data-management services
- Monitoring and troubleshooting
- Messaging and eventing
- Vector databases
- Python programming
- Containerized applications on Azure

### Blueprint maintenance rule

Because certification outlines can change, the official study guide must be checked:

- Before beginning Week 1
- At the end of Week 4
- Before scheduling the exam
- Whenever Microsoft publishes an updated skills outline

When an objective changes, update the competency matrix and the relevant weekly playbook before continuing.

### Official references

- [AI-200 study guide](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-200)
- [Azure AI Cloud Developer Associate certification](https://learn.microsoft.com/en-us/credentials/certifications/azure-ai-cloud-developer-associate/)
- [AI-200 official course](https://learn.microsoft.com/en-us/training/courses/ai-200t00)

---

## 3. What AI-200 Actually Emphasizes

Despite the certification name, the current AI-200 outline is not primarily an Azure OpenAI, Microsoft Foundry, Semantic Kernel, or prompt-engineering exam.

Its assessed skills emphasize the Azure back end required to host, integrate, secure, and operate AI-driven applications:

- Azure Container Registry and ACR Tasks
- Containers on Azure App Service
- Azure Container Apps
- KEDA event-driven scaling
- Azure Kubernetes Service
- Azure Cosmos DB for NoSQL
- Azure Database for PostgreSQL and `pgvector`
- Azure Managed Redis
- Vector storage and similarity search
- Azure Service Bus
- Azure Event Grid
- Azure Functions
- Azure Key Vault
- Azure App Configuration
- OpenTelemetry
- Kusto Query Language

Technologies such as Azure OpenAI, Microsoft Foundry, Azure AI Search, Semantic Kernel, Prompt Flow, Blob Storage, Managed Identity, Bicep, and GitHub Actions can strengthen the engineering project. They must not, however, replace explicit AI-200 objectives.

### Topic classification

Every technology added to the residency belongs to one of three categories:

| Classification | Meaning |
|---|---|
| **Exam-core** | Explicitly named in the current official skills outline. Must be studied, implemented, assessed, and reviewed. |
| **Exam-supporting** | Helps implement an exam objective but is not independently named in the outline. |
| **Career-extension** | Valuable for the broader Senior Azure AI Engineer path but secondary until exam-core coverage is secure. |

Examples:

| Topic | Classification |
|---|---|
| Azure Container Registry Tasks | Exam-core |
| Azure Container Apps revisions | Exam-core |
| AKS manifests | Exam-core |
| Cosmos DB change feed processor | Exam-core |
| PostgreSQL `pgvector` | Exam-core |
| Azure Managed Redis vector indexing | Exam-core |
| Service Bus dead-letter queues | Exam-core |
| OpenTelemetry and KQL | Exam-core |
| Docker fundamentals | Exam-supporting |
| Managed Identity | Exam-supporting |
| Blob Storage for FIT files | Exam-supporting |
| Azure OpenAI embedding generation | Exam-supporting |
| Microsoft Foundry | Career-extension |
| Semantic Kernel | Career-extension |
| Azure AI Search | Career-extension |
| Prompt Flow | Career-extension |

### Priority rule

When time is limited:

1. Exam-core objectives come first.
2. Zapas engineering quality comes second.
3. Exam-supporting skills come third.
4. Career-extension topics are postponed rather than allowed to create a coverage gap.

This rule does not make the residency shallow. It prevents attractive but currently unassessed technologies from displacing the skills the certification actually measures.

---

## 4. Vision of Graduation

At graduation, the resident should be able to:

- Pass AI-200 through genuine understanding of every measured domain.
- Build, version, store, deploy, and troubleshoot container images on Azure.
- Compare App Service, Container Apps, and AKS using technical and operational trade-offs.
- Design data access for AI workloads with Cosmos DB, PostgreSQL, and Redis.
- Implement vector storage, similarity search, semantic retrieval, and RAG-oriented data flows.
- Build reliable event-driven workflows with Service Bus and Event Grid.
- Build and deploy Azure Functions.
- Secure secrets and configuration using the services named in the exam outline.
- Trace distributed systems with OpenTelemetry.
- Investigate failures using logs, metrics, events, connectivity checks, and KQL.
- Read and modify relevant Python SDK code.
- Explain architecture decisions with senior-level clarity.
- Distinguish a service that belongs in the final Zapas architecture from a service that was used only for an exam-focused lab.
- Remove unnecessary infrastructure rather than turning the capstone into a catalog of Azure services.

The residency should produce both:

- **Exam confidence:** recognition, recall, configuration knowledge, and scenario judgment.
- **Engineering confidence:** implementation, troubleshooting, trade-off analysis, and communication.

---

## 5. Residency Philosophy

### 5.1 The official outline defines coverage

The residency may enrich the outline, but it may not silently omit, rename, or replace it.

A week is not complete merely because a Zapas feature works. The related exam objectives must also have been:

1. Studied
2. Implemented
3. Explained
4. Troubleshot
5. Practiced in exam-style scenarios
6. Recorded in the competency matrix

### 5.2 Zapas is the learning spine, not a constraint

Most skills should be learned through Zapas. However, forcing every Azure service into one permanent production architecture would create a poor design.

Some objectives will therefore use:

- A temporary deployment
- A focused Git branch
- A small companion service
- A disposable Azure resource group
- A Python SDK lab
- A controlled comparison experiment

The final Zapas architecture retains only services justified by its requirements.

### 5.3 Building and exam preparation reinforce each other

Building creates procedural knowledge. Exam practice creates recognition, recall, and decision-making under constraints.

Neither is sufficient by itself:

- Building without an objective map creates coverage gaps.
- Reading without building creates shallow familiarity.
- Question banks without reflection create answer-pattern memorization.
- Architecture discussion without operational practice hides implementation weaknesses.

### 5.4 Every service must answer two questions

Before introducing a service, record:

1. **Which official AI-200 objective does this exercise?**
2. **Which Zapas requirement or focused lab scenario justifies it?**

When a service answers neither question, postpone it.

### 5.5 Exam readiness is demonstrated, not assumed

Feeling familiar with a service is not enough. The resident must be able to:

- Recognize when the service is appropriate
- Reject plausible but inferior alternatives
- Configure the relevant feature
- Interpret a failure
- Recall important behavior
- Complete a representative implementation without copying a tutorial
- Explain why distractors are wrong

### 5.6 Troubleshooting is part of learning

Every domain includes intentional failure exercises. A deployment that has never failed teaches less than one that has been broken, observed, diagnosed, and repaired.

---

## 6. Product and Language Strategy

## 6.1 Primary engineering track: Zapas in .NET

Zapas remains an ASP.NET Core application and the main engineering artifact.

Its existing domain makes it suitable for realistic cloud evolution:

- Uploading `.fit` activity files
- Validating and parsing activities
- Storing sessions and intervals
- Performing asynchronous processing
- Generating searchable activity insights
- Exposing APIs
- Caching expensive retrieval
- Publishing processing events
- Monitoring failures and latency

The residency will progressively add or experiment with:

- Container packaging and registries
- Azure hosting targets
- AI-oriented data stores
- Vector retrieval
- Messaging and events
- Serverless processing
- Secure configuration
- Distributed tracing
- Operational diagnostics

## 6.2 Python exam-fluency track

The official candidate profile explicitly expects Python programming proficiency. An all-.NET residency would therefore leave a real exam risk.

The solution is not to rewrite Zapas in Python.

Instead, every week includes a focused **Python exam-fluency lab** using the same domain and Azure resources whenever practical.

Examples:

- Query Cosmos DB from Python
- Store and retrieve PostgreSQL vectors from Python
- Perform Redis cache operations from Python
- Send and receive Service Bus messages from Python
- Publish an Event Grid custom event from Python
- Implement an Azure Function in Python
- Retrieve a secret from Key Vault in Python
- Add OpenTelemetry instrumentation to a Python component

Recommended repository structure:

```text
zapas/
├── src/
│   ├── Zapas.Api/
│   ├── Zapas.Application/
│   ├── Zapas.Domain/
│   └── Zapas.Infrastructure/
├── workers/
│   ├── Zapas.Worker/
│   └── zapas-python-worker/
├── functions/
│   └── zapas-python-functions/
├── exam-labs/
│   ├── week01-container-images/
│   ├── week02-orchestration/
│   ├── week03-cosmos/
│   ├── week04-postgresql/
│   ├── week05-redis/
│   ├── week06-messaging-functions/
│   └── week07-security-observability/
└── residency/
    ├── exam_objective_matrix.md
    ├── exam_error_log.md
    ├── exam_notes.md
    ├── engineering_wisdom.md
    ├── architecture_evolution.md
    ├── decision_log.md
    ├── tech_lead_reviews.md
    ├── interview_journal.md
    └── cost_and_cleanup_log.md
```

### Python target

The resident does not need to become a Python specialist in eight weeks. The target is to become comfortable enough to:

- Read Python SDK examples
- Modify a working sample
- Write basic asynchronous code
- Configure clients and credentials
- Handle exceptions
- Work with dictionaries, lists, classes, and environment variables
- Implement short Azure service operations
- Understand exam questions containing Python

Python practice begins in Week 1 and continues throughout the residency.

---

## 7. Official AI-200 Competency Matrix

Each objective receives an identifier used in weekly playbooks, reviews, and the progress tracker.

## Domain C — Develop containerized solutions on Azure

| ID | Official competency |
|---|---|
| C1 | Build, store, version, and manage container images by using Azure Container Registry |
| C2 | Build and run images by using Azure Container Registry Tasks |
| C3 | Deploy containers to Azure App Service, including configuring environment variables and secrets |
| C4 | Deploy applications to Azure Container Apps, including environment configuration and revision management |
| C5 | Implement event-driven scaling by using KEDA in Container Apps |
| C6 | Deploy and manage applications to AKS by using manifest files |
| C7 | Monitor and troubleshoot AKS and Container Apps by inspecting logs, events, and end-to-end connectivity |

## Domain D — Develop AI solutions by using Azure data management services

| ID | Official competency |
|---|---|
| D1 | Connect to Cosmos DB for NoSQL by using the SDK and run queries |
| D2 | Optimize Cosmos DB query performance and RU consumption by using indexing policies and consistency levels |
| D3 | Store and retrieve embeddings and execute vector similarity search in Cosmos DB |
| D4 | Implement a Cosmos DB change feed processor |
| D5 | Connect to and query Azure Database for PostgreSQL by using SDKs |
| D6 | Model PostgreSQL schemas and choose appropriate tables, data types, and indexes |
| D7 | Optimize PostgreSQL vector queries and reduce `pgvector` compute overhead |
| D8 | Configure PostgreSQL compute, memory, and storage for vector workloads |
| D9 | Store embeddings, run vector similarity search, apply metadata filters, and implement RAG-oriented retrieval in PostgreSQL |
| D10 | Optimize PostgreSQL connections for throughput and latency |
| D11 | Implement Azure Managed Redis data operations, caching, expiration, and invalidation |
| D12 | Implement Azure Managed Redis vector indexing and similarity search |

## Domain S — Connect to and consume Azure services

| ID | Official competency |
|---|---|
| S1 | Queue and process back-end operations by using Service Bus |
| S2 | Use Service Bus dead-letter queues, messages, topics, and subscriptions |
| S3 | Implement Event Grid workflows with filters, custom events, and retries |
| S4 | Build serverless APIs with Azure Functions triggers and bindings |
| S5 | Configure and deploy Azure Function Apps |

## Domain O — Secure, monitor, and troubleshoot Azure solutions

| ID | Official competency |
|---|---|
| O1 | Secure secrets by using Key Vault, including retrieval and rotation |
| O2 | Store and retrieve application configuration by using Azure App Configuration |
| O3 | Trace distributed systems by using OpenTelemetry SDKs |
| O4 | Write KQL queries to analyze logs and metrics |

---

## 8. Competency Levels and Evidence

Each objective is tracked with the following scale:

| Level | Meaning |
|---:|---|
| 0 | Not introduced |
| 1 | Recognize the terminology |
| 2 | Explain the purpose and common trade-offs |
| 3 | Implement the skill with limited reference material |
| 4 | Implement, troubleshoot, and explain the skill independently |
| 5 | Compare alternatives, diagnose ambiguous scenarios, and teach the skill |

### Exam-ready threshold

An objective is considered exam-ready when:

- It is at Level 3 or higher.
- It has been answered correctly in more than one scenario format.
- At least one common failure mode has been investigated.
- The resident can explain why two plausible distractors are incorrect.
- Practical evidence exists in the repository or Azure environment notes.

High-risk objectives should reach Level 4:

- C4–C7: Container Apps, KEDA, AKS, and troubleshooting
- D2–D4: Cosmos DB optimization, vectors, and change feed
- D7–D10: PostgreSQL vector optimization and connectivity
- S1–S3: Service Bus and Event Grid reliability behavior
- O3–O4: OpenTelemetry and KQL

### Evidence types

Acceptable evidence includes:

- Working code
- A deployed resource
- A reproducible command sequence
- Configuration captured in source control
- A troubleshooting report
- An architecture diagram
- A query or manifest written from memory
- A passing weekly assessment
- A verbal explanation recorded in notes

A copied tutorial is not sufficient evidence until the resident can reproduce or modify it independently.

---

## 9. Residency Phases

## Phase 1 — Containerized Compute

**Weeks 1–2**

### Goal

Cover the complete containerized-solutions domain early and establish the deployment foundation used throughout the residency.

### Outcomes

- Containerize Zapas.
- Manage images in Azure Container Registry.
- Use ACR Tasks.
- Deploy a container to Azure App Service.
- Deploy and revise Zapas in Azure Container Apps.
- Configure KEDA-based scaling.
- Deploy a focused Zapas workload to AKS using manifests.
- Troubleshoot container logs, events, configuration, and connectivity.

## Phase 2 — AI Data Plane

**Weeks 3–5**

### Goal

Cover the largest exam domain by implementing and comparing the three data technologies explicitly named in the skills outline.

### Outcomes

- Use Cosmos DB for NoSQL from SDKs.
- Reason about RUs, indexing, consistency, vector search, and change feed.
- Use Azure Database for PostgreSQL and `pgvector`.
- Design relational and vector schemas.
- Tune vector workloads and connections.
- Use Azure Managed Redis for caching and vector indexing.
- Make a defensible data-architecture decision for Zapas.

## Phase 3 — Integration and Serverless Processing

**Week 6**

### Goal

Connect Zapas services through messages, events, and functions while mastering reliability behavior.

### Outcomes

- Implement Service Bus queues, topics, subscriptions, and dead-letter handling.
- Implement Event Grid custom events, filtering, and retries.
- Build and deploy Python Azure Functions with triggers and bindings.
- Explain when to choose Service Bus, Event Grid, or Functions.

## Phase 4 — Secure Operations and Exam Mastery

**Weeks 7–8**

### Goal

Secure, observe, troubleshoot, integrate, and defend the complete solution while closing exam gaps.

### Outcomes

- Retrieve and rotate secrets with Key Vault.
- Centralize dynamic configuration with App Configuration.
- Trace distributed flows with OpenTelemetry.
- Analyze logs and metrics with KQL.
- Complete integrated troubleshooting exercises.
- Complete timed exam simulations.
- Reach the exam-ready threshold across all objectives.

---

## 10. Eight-Week Coverage Dashboard

| Week | Engineering focus | AI-200 objectives | Primary evidence |
|---:|---|---|---|
| 1 | Image lifecycle and App Service containers | C1–C3 | ACR images, ACR Task, App Service deployment |
| 2 | Container Apps, KEDA, AKS, troubleshooting | C4–C7 | Revisions, scaling rule, manifests, runbook |
| 3 | Cosmos DB and vector data | D1–D4 | Queries, RU experiment, vector search, change feed |
| 4 | PostgreSQL, `pgvector`, and RAG-oriented retrieval | D5–D10 | Schema, indexes, vector queries, connection tuning |
| 5 | Azure Managed Redis | D11–D12 | Cache behavior, invalidation, vector index |
| 6 | Service Bus, Event Grid, and Functions | S1–S5 | Async workflow, DLQ, events, Python Function |
| 7 | Key Vault, App Configuration, OpenTelemetry, KQL | O1–O4 | Secret rotation, configuration, traces, KQL workbook |
| 8 | Integration and exam readiness | C1–C7, D1–D12, S1–S5, O1–O4 | Capstone, objective audit, mock assessments |

### Weighting interpretation

The weekly distribution intentionally gives:

- Two full weeks to containerized solutions
- Three full weeks to the largest data-management domain
- One focused week to messaging and functions
- One focused week to security and observability
- One integration and exam-repair week

This reflects the official weights while keeping enough repetition across later weeks to prevent early objectives from being forgotten.

---

# 11. Weekly Roadmap

## Week 1 — Container Image Lifecycle and App Service Containers

### Engineering question

> How do we package Zapas consistently, manage its images, and run the container on an Azure managed web platform?

### Primary exam domain

**Develop containerized solutions on Azure**

### Objectives

- C1 — Azure Container Registry image lifecycle
- C2 — Azure Container Registry Tasks
- C3 — Azure App Service container deployment, environment variables, and secrets

### Zapas evolution

- Create a production-oriented multi-stage Dockerfile.
- Define container-safe application configuration.
- Add or verify a health endpoint.
- Build and run Zapas locally.
- Tag images intentionally.
- Push images to Azure Container Registry.
- Use ACR Tasks to build an image in Azure.
- Deploy the Zapas API container to Azure App Service.
- Configure environment variables and secret values.
- Validate startup, health, logs, and connectivity.
- Document image versioning and rollback behavior.

### Python exam-fluency lab

- Build a minimal Python API container.
- Read configuration from environment variables.
- Build and push the image with ACR.
- Compare Python and .NET container entry points.

### Exam study focus

- Distinguish an image, registry, repository, tag, digest, and task.
- Distinguish a local Docker build from an ACR Task.
- Understand image versioning and deployment selection.
- Recognize App Service container configuration scenarios.
- Diagnose an image-pull failure, startup failure, incorrect port, or missing setting.

### Required deliverables

- `Dockerfile`
- `.dockerignore`
- ACR repository with versioned Zapas images
- Reproducible ACR Task commands or definition
- App Service container deployment
- `ADR-001-container-image-strategy.md`
- Week 1 exam notes for C1–C3
- At least 20 exam-style questions
- One troubleshooting report

### Weekly gate

The resident must be able to:

- Build and push a new image version without a tutorial.
- Explain the full image lifecycle.
- Deploy a selected version to App Service.
- Configure and verify environment variables and secrets.
- Diagnose one intentionally broken deployment.

---

## Week 2 — Container Apps, KEDA, AKS, and Troubleshooting

### Engineering question

> When should Zapas run on Container Apps or AKS, and how do we deploy, scale, and troubleshoot each platform?

### Primary exam domain

**Develop containerized solutions on Azure**

### Objectives

- C4 — Container Apps environments and revision management
- C5 — KEDA event-driven scaling
- C6 — AKS deployments with manifest files
- C7 — Monitoring and troubleshooting AKS and Container Apps

### Zapas evolution

- Deploy Zapas to Azure Container Apps.
- Configure ingress, environment variables, and secrets.
- Create a new revision.
- Control traffic between revisions.
- Roll back to a previous revision.
- Add a containerized FIT-processing worker.
- Configure KEDA scaling from an appropriate event source.
- Observe scale-to-zero and scale-out behavior.
- Create focused Kubernetes manifests for:
  - Deployment
  - Service
  - ConfigMap
  - Secret reference
- Deploy the API or worker to a temporary AKS environment.
- Inspect pod logs, deployment status, services, events, and connectivity.
- Break and diagnose at least three scenarios:
  - Incorrect image or image-pull failure
  - Missing or incorrect configuration
  - Failed service-to-service connectivity

### Architecture rule

AKS is an exam objective, but it does not have to become the permanent Zapas hosting platform. The AKS environment may be temporary and should be removed after the lab to control cost.

### Python exam-fluency lab

- Deploy the Week 1 Python container to Container Apps.
- Read a KEDA-related scaling configuration.
- Use basic Kubernetes commands to inspect the Python pod.

### Exam study focus

- Compare App Service containers, Container Apps, and AKS.
- Distinguish a Container Apps environment, app, replica, and revision.
- Understand revision modes and traffic splitting.
- Explain how KEDA uses event sources to determine scaling.
- Read and interpret Kubernetes manifest fragments.
- Select the correct log, event, or connectivity check for a described failure.

### Required deliverables

- Container Apps deployment
- Revision and traffic-management demonstration
- KEDA scaling configuration
- AKS manifest set
- Container troubleshooting runbook
- `ADR-002-hosting-platform.md`
- Week 2 exam notes for C4–C7
- At least 30 exam-style questions
- Full container-domain checkpoint assessment

### Weekly gate

The resident must be able to:

- Deploy a revision and roll it back.
- Explain event-driven scaling versus CPU-based scaling.
- Read a basic Kubernetes manifest without assistance.
- Locate the correct diagnostic surface for a container failure.
- Defend the final hosting choice for Zapas.
- Reach Level 3 or higher on C1–C7.

---

## Week 3 — Cosmos DB for NoSQL and Vector Data

### Engineering question

> How can Zapas store AI-oriented documents, embeddings, and changes efficiently in Cosmos DB?

### Primary exam domain

**Develop AI solutions by using Azure data management services**

### Objectives

- D1 — Cosmos DB SDK connections and queries
- D2 — RU optimization, indexing policies, and consistency levels
- D3 — Embeddings and vector similarity search
- D4 — Change feed processor

### Zapas evolution

Create a focused Cosmos DB-backed **activity-insight document store**:

- Store denormalized activity summaries.
- Query records through the SDK.
- Select and justify a partition key.
- Measure Request Unit consumption.
- Compare point reads and queries.
- Modify an indexing policy.
- Compare relevant consistency options.
- Store embeddings with activity metadata.
- Execute vector similarity searches.
- Process new or changed activity documents through the change feed.
- Produce a downstream derived result, event, or audit record.

An embedding provider may be used as supporting infrastructure. It must not replace the database objectives.

### Python exam-fluency lab

- Create a Cosmos client.
- Insert and query activity documents.
- Perform a parameterized query.
- Read request-charge information.
- Run a vector query.
- Implement or modify a change-feed handler.

### Exam study focus

- Partition-key selection
- Point reads versus queries
- Cross-partition behavior
- RUs and query shape
- Indexing-policy effects
- Consistency-level trade-offs
- Vector-field and similarity-search concepts
- Change feed versus polling

### Required deliverables

- Cosmos DB container and data model
- Query and RU observation notes
- Custom indexing-policy experiment
- Vector-search demonstration
- Change-feed processor
- `ADR-003-cosmos-partition-and-consistency.md`
- Week 3 exam notes for D1–D4
- At least 30 exam-style questions

### Weekly gate

The resident must be able to:

- Select and defend a partition key.
- Identify why a query consumes excessive RUs.
- Choose an appropriate consistency model for a scenario.
- Store and query vector data.
- Explain when change feed is preferable to polling.
- Diagnose an inefficient or broken Cosmos query.

---

## Week 4 — PostgreSQL, pgvector, and RAG-Oriented Retrieval

### Engineering question

> How should Zapas model relational and vector data in PostgreSQL, and how do we optimize semantic retrieval?

### Primary exam domain

**Develop AI solutions by using Azure data management services**

### Objectives

- D5 — PostgreSQL SDK connections and queries
- D6 — Schema, table, data type, and index design
- D7 — Vector-query optimization and `pgvector` overhead
- D8 — Compute, memory, and storage for vector workloads
- D9 — Embeddings, similarity search, metadata filters, and RAG patterns
- D10 — Connection optimization

### Zapas evolution

- Provision or connect to Azure Database for PostgreSQL.
- Model activity, interval, insight, and embedding records.
- Enable and use `pgvector`.
- Select vector and metadata column types.
- Implement similarity search.
- Add metadata filters such as athlete, activity date, distance, or workout type.
- Compare appropriate search and indexing approaches.
- Create and evaluate relevant indexes.
- Observe query plans and latency.
- Configure connection pooling and timeouts.
- Test concurrency and connection-exhaustion behavior.
- Build a small RAG-oriented retrieval flow that retrieves relevant activity context before returning or generating an answer.

The focus is the data and retrieval layer. A model endpoint may support the demonstration, but model prompting is not the principal objective.

### Python exam-fluency lab

- Connect with a Python PostgreSQL driver.
- Create or query vector records.
- Parameterize a similarity query.
- Add metadata filtering.
- Use a connection pool.
- Handle timeout and transient connection errors.

### Exam study focus

- Relational schema choices
- Data-type selection
- General and vector indexes
- Causes of vector-query overhead
- Compute, memory, and storage decisions
- Metadata-filtered retrieval
- Connection pooling, throughput, and latency
- Cosmos DB versus PostgreSQL scenarios

### Required deliverables

- PostgreSQL schema
- Migration or reproducible setup
- Vector-retrieval endpoint or service
- Metadata-filtered semantic search
- Query-plan and latency notes
- Connection-pooling configuration
- RAG-oriented retrieval demonstration
- `ADR-004-ai-data-store-selection.md`
- Week 4 exam notes for D5–D10
- At least 35 exam-style questions
- Midpoint official-outline verification

### Weekly gate

The resident must be able to:

- Design a reasonable relational and vector schema.
- Choose and explain an indexing approach.
- Diagnose a slow vector query.
- Explain how metadata filtering affects quality and performance.
- Explain the roles of compute, memory, storage, and connections.
- Compare PostgreSQL and Cosmos DB without declaring a universal winner.

---

## Week 5 — Azure Managed Redis, Caching, and Vector Indexing

### Engineering question

> Where can Redis reduce latency in Zapas, and when can it serve as a vector retrieval layer?

### Primary exam domain

**Develop AI solutions by using Azure data management services**

### Objectives

- D11 — Redis operations, caching, expiration, and invalidation
- D12 — Redis vector indexing and similarity search

### Zapas evolution

- Connect to Azure Managed Redis.
- Cache an expensive Zapas query or generated activity insight.
- Define key naming and serialization rules.
- Implement expiration.
- Implement explicit invalidation.
- Demonstrate stale-data behavior.
- Implement cache-aside behavior and safe failure handling.
- Create a vector index.
- Store vector records with searchable metadata.
- Execute similarity searches.
- Compare Redis vector retrieval with the PostgreSQL and Cosmos DB experiments.
- Decide whether Redis belongs in the permanent Zapas architecture.

### Python exam-fluency lab

- Set and retrieve values.
- Configure expiration.
- Delete or invalidate keys.
- Handle a cache miss.
- Create or query a vector index.
- Catch connection failures and degrade safely.

### Exam study focus

- Caching versus durable persistence
- Expiration versus invalidation
- Cache-aside behavior
- Consistency and stale-data risks
- Vector indexing in Redis
- Comparison of Cosmos DB, PostgreSQL, and Redis for AI workloads

### Required deliverables

- Redis-backed cache
- Invalidation demonstration
- Vector index and query
- Data-service comparison matrix
- `ADR-005-cache-and-vector-retrieval.md`
- Week 5 exam notes for D11–D12
- At least 25 exam-style questions
- Full data-domain checkpoint assessment

### Weekly gate

The resident must be able to:

- Implement a cache without treating it as the source of truth.
- Explain expiration versus invalidation.
- Diagnose stale-cache and unavailable-cache scenarios.
- Create and query a vector index.
- Select among Cosmos DB, PostgreSQL, and Redis for a described workload.
- Reach Level 3 or higher on D1–D12.

---

## Week 6 — Service Bus, Event Grid, and Azure Functions

### Engineering question

> How should Zapas coordinate reliable back-end work, react to events, and expose serverless operations?

### Primary exam domain

**Connect to and consume Azure services**

### Objectives

- S1 — Service Bus back-end processing
- S2 — Dead-letter queues, messages, topics, and subscriptions
- S3 — Event Grid filters, custom events, and retries
- S4 — Azure Functions triggers and bindings
- S5 — Function App configuration and deployment

### Zapas evolution

Design an asynchronous FIT-import workflow:

1. The API accepts an upload request.
2. It stores the required durable data or reference.
3. It sends a command through Service Bus.
4. A worker processes the import.
5. Failure and retry behavior are observable.
6. Poison messages are moved to or inspected in a dead-letter queue.
7. A completed-import event is published.
8. Event Grid routes selected events.
9. A Python Azure Function handles a notification, projection, or maintenance task.

Implement exercises for:

- Service Bus queue
- Topic and subscription
- Dead-letter handling
- Duplicate-safe message processing
- Event Grid custom event
- Event filtering
- Retry behavior
- Azure Function trigger
- Azure Function binding
- Function App configuration and deployment

### Python exam-fluency lab

This week’s primary Function should be implemented in Python.

The resident should also be able to:

- Send and receive a Service Bus message.
- Publish a custom Event Grid event.
- Read trigger metadata.
- Use application settings.
- Deploy and invoke the Function App.

### Exam study focus

- Queue versus topic and subscription
- Service Bus versus Event Grid
- Commands versus events
- Delivery, locks, retries, duplicate processing, and dead-letter behavior
- Trigger and binding selection
- Function App configuration and deployment
- Diagnosing a function that cannot read configuration or process an event

### Required deliverables

- Service Bus queue workflow
- Topic and subscription lab
- Dead-letter handling procedure
- Event Grid custom event and filter
- Deployed Python Function App
- Messaging sequence diagram
- `ADR-006-messaging-and-eventing.md`
- Week 6 exam notes for S1–S5
- At least 40 exam-style questions
- Messaging-and-functions checkpoint assessment

### Weekly gate

The resident must be able to:

- Choose Service Bus or Event Grid for a scenario.
- Explain when a topic is preferable to a queue.
- Inspect and resolve or reprocess a dead-lettered message.
- Build a basic Python Function from memory.
- Identify why a trigger or binding is not functioning.
- Reach Level 3 or higher on S1–S5.

---

## Week 7 — Key Vault, App Configuration, OpenTelemetry, and KQL

### Engineering question

> How do we secure Zapas configuration, trace distributed work, and investigate production failures?

### Primary exam domain

**Secure, monitor, and troubleshoot Azure solutions**

### Objectives

- O1 — Key Vault secret retrieval and rotation
- O2 — Azure App Configuration
- O3 — OpenTelemetry distributed tracing
- O4 — KQL analysis of logs and metrics

### Zapas evolution

- Move sensitive values to Key Vault.
- Retrieve secrets securely.
- Demonstrate a rotation process.
- Move non-secret dynamic settings to Azure App Configuration.
- Distinguish configuration values, feature flags, and secrets.
- Instrument the API, worker, and Function with OpenTelemetry.
- Propagate trace context through the asynchronous workflow where practical.
- Export telemetry to an Azure monitoring destination.
- Write KQL queries for:
  - Failed requests
  - High-latency operations
  - Dependency failures
  - Import-processing errors
  - Dead-letter activity
  - Trace correlation
- Investigate an end-to-end failure across multiple components.

Managed Identity may be used as a supporting secure-access technique, but the formal assessment focus remains Key Vault and App Configuration.

### Python exam-fluency lab

- Retrieve a Key Vault secret.
- Read an App Configuration value.
- Instrument a Python Function or worker with OpenTelemetry.
- Correlate a Python operation with the wider trace.
- Read and explain KQL query results.

### Exam study focus

- Secrets versus configuration
- Secret retrieval and rotation
- App Configuration usage
- OpenTelemetry traces, spans, context, attributes, and propagation
- KQL filters, projections, aggregations, time windows, and correlations
- Diagnosing missing or disconnected telemetry

### Required deliverables

- Key Vault integration
- Documented secret-rotation exercise
- App Configuration integration
- End-to-end distributed trace
- KQL workbook with at least 15 purposeful queries
- Production-incident report
- `ADR-007-secrets-configuration-and-observability.md`
- Week 7 exam notes for O1–O4
- At least 35 exam-style questions
- Security-and-observability checkpoint assessment

### Weekly gate

The resident must be able to:

- Choose Key Vault or App Configuration correctly.
- Explain and demonstrate a rotation workflow.
- Follow one request across multiple services.
- Write KQL for a new diagnostic question.
- Diagnose missing or disconnected telemetry.
- Reach Level 3 or higher on O1–O4.

---

## Week 8 — Integration, Weak-Area Repair, and Exam Readiness

### Engineering question

> Can I recognize, implement, troubleshoot, and defend every AI-200 objective under exam conditions?

### Primary exam domain

**All domains**

### Objectives

- C1–C7
- D1–D12
- S1–S5
- O1–O4

### Zapas capstone

Demonstrate a coherent end-to-end system. A possible final architecture is:

```text
Client
  |
  v
Zapas API on Azure Container Apps
  |
  +--> Primary operational data store
  |
  +--> Service Bus command
          |
          v
      Containerized worker
          |
          +--> Vector-capable data service
          +--> Redis cache or vector index when justified
          +--> Completion event
                    |
                    v
              Event Grid
                    |
                    v
              Python Azure Function

Shared operational services:
- Azure Container Registry
- Key Vault
- App Configuration
- OpenTelemetry export
- Azure logs and metrics queried with KQL
```

The final architecture does not have to retain every service used during the residency. The resident must identify:

- Services retained in production
- Services used only for objective-focused labs
- Services rejected after comparison
- Cost and operational consequences
- Simplification opportunities

### Week 8 sequence

#### Day 1 — Objective audit

- Recheck the official study guide.
- Update every objective’s competency level.
- Identify the five weakest objectives.
- Build a repair plan.

#### Day 2 — Container and data simulation

- Complete timed scenario sets for Domains C and D.
- Perform one container deployment from memory.
- Perform one vector-data task from memory.
- Review every error.

#### Day 3 — Messaging, Functions, security, and monitoring simulation

- Complete timed scenario sets for Domains S and O.
- Deploy or repair one Function.
- Resolve one messaging failure.
- Write KQL for an unseen incident.

#### Day 4 — Full mock and repair

- Complete a full timed mock.
- Categorize every error:
  - Knowledge gap
  - Misread requirement
  - Confused services
  - Configuration detail
  - Time pressure
  - Changed answer without evidence
- Repair the three most damaging categories.

#### Day 5 — Final defense and exam plan

- Conduct the final Tech Lead review.
- Explain the entire architecture in ten minutes.
- Answer rapid scenario questions.
- Review the official exam sandbox.
- Finalize exam-day strategy.

### Required deliverables

- Completed competency matrix
- Final architecture diagram
- Final decision log
- Final exam error log
- Two full timed mock results
- Weak-area repair notes
- Capstone demonstration
- Ten-minute architecture presentation
- Final exam-readiness report

### Graduation gate

The resident is ready to schedule the exam when:

- Every official objective is at Level 3 or higher.
- High-risk objectives are at Level 4.
- No domain remains materially weaker than the others.
- Two fresh timed practice sets show stable performance.
- Representative tasks can be completed without a tutorial.
- Plausible distractors can be explained and rejected.
- The official study guide has been checked for changes.

A practice score of 80% or higher can be used as a readiness heuristic. It is not equivalent to Microsoft’s scaled exam score.

---

## 12. Weekly Learning Method

Each week follows the same five-day rhythm.

### Monday — Objective and mental model

- Read the exact official objectives.
- Complete the relevant official learning material.
- Build a terminology map.
- Identify similar Azure services and likely distractors.
- Begin the smallest working implementation.

### Tuesday — Core implementation

- Build the principal Zapas capability.
- Capture configuration steps.
- Record errors and corrections.
- Explain the implementation aloud.

### Wednesday — Variations and Python

- Complete the Python exam-fluency lab.
- Modify the implementation without following the original guide.
- Compare at least two alternatives.
- Add an ADR when the decision is architecturally meaningful.

### Thursday — Failure and exam scenarios

- Break the implementation intentionally.
- Diagnose it through the correct Azure diagnostic surfaces.
- Complete scenario-based questions.
- Add mistakes to `exam_error_log.md`.
- Review weak concepts using active recall.

### Friday — Synthesis and gate

- Demonstrate the week’s build.
- Complete a timed weekly assessment.
- Conduct the Tech Lead review.
- Update the competency matrix.
- Decide whether any objective requires carry-over work.

A week must not silently pass with missing objectives. Carry-over work must be explicitly scheduled.

---

## 13. Daily Learning Framework

Every study day follows this sequence.

### 1. Objective

Read the exact AI-200 objective being practiced.

Answer:

- What does Microsoft explicitly expect?
- What terminology is likely to appear in an exam scenario?
- What adjacent services could become distractors?

### 2. Problem

Start from a Zapas requirement or a focused lab scenario.

Do not begin with “Today I will learn Service X.” Begin with a problem that Service X may or may not solve.

### 3. Design

- Compare realistic alternatives.
- Identify constraints.
- Predict cost and operational effects.
- Write or update an ADR when appropriate.

### 4. Build

Implement the skill in Zapas or the week’s focused lab by following a complete, reproducible, step-by-step implementation that includes all required code, commands, configuration, validation checkpoints, troubleshooting guidance, and cleanup instructions.

No implementation counts until it runs and its configuration is understood.

### 5. Break and troubleshoot

Create a controlled failure and diagnose it using the correct Azure tools.

### 6. Explain

Explain aloud:

- Why this solution?
- Why not the nearest alternatives?
- What is the exam-relevant configuration?
- What can fail?
- Where would you investigate first?
- Should this remain in the final Zapas architecture?

### 7. Retrieve

Close the documentation and reproduce one key action, diagram, query, or explanation from memory.

### 8. Reflect

Record:

- What I learned
- What I confused
- What changed my mind
- What remains weak
- What should be reviewed later

---

## 14. Suggested Time Allocation

The blueprint uses proportions so it can adapt to the resident’s available time.

| Activity | Share |
|---|---:|
| Zapas or focused hands-on implementation | 40% |
| Official objective study and documentation | 20% |
| Python SDK fluency | 15% |
| Exam-style questions and active recall | 15% |
| Explanation, ADRs, and reflection | 10% |

### Example two-hour session

- 45 minutes implementation
- 25 minutes official study
- 20 minutes Python
- 20 minutes questions and recall
- 10 minutes explanation and reflection

### Rule against passive study

Watching a video or reading documentation does not complete an objective.

Every study block must produce at least one artifact:

- Code
- Command sequence
- Configuration
- Diagram
- Explanation
- Quiz result
- Error-log entry
- ADR
- Troubleshooting note

---

## 15. Exam Preparation System

## 15.1 Objective tracker

Maintain `exam_objective_matrix.md`:

| ID | Week | Level | Last practiced | Evidence | Weakness | Next action |
|---|---:|---:|---|---|---|---|

Update it every Friday.

## 15.2 Exam error log

Maintain `exam_error_log.md`.

For every incorrect question, record:

- Domain and objective ID
- Question type
- Chosen answer
- Correct principle
- Why the chosen answer was attractive
- Why it was wrong
- Rule or distinction to remember
- Review date

Do not copy entire commercial questions into the repository. Record the lesson in your own words.

## 15.3 Retrieval practice

Use spaced review:

- Same day
- Two days later
- One week later
- Three weeks later
- During Week 8

Review should use questions, blank diagrams, commands from memory, and scenario decisions—not rereading alone.

## 15.4 Question quality

Prefer questions that require:

- Choosing among similar Azure services
- Sequencing implementation steps
- Identifying configuration errors
- Interpreting code, manifests, or queries
- Diagnosing logs and symptoms
- Selecting the most appropriate operational solution

Do not rely only on definition questions.

## 15.5 Practice resources

Use:

- The current official study guide
- The official AI-200 course and Microsoft Learn modules
- Microsoft product documentation
- The official exam sandbox
- Original scenario questions created from the skills outline
- Hands-on timed labs
- Reputable practice material explicitly mapped to the current outline

Do not use leaked questions or exam dumps.

---

## 16. Friday Tech Lead and Certification Review

Every Friday review has two equal halves.

## Part A — Engineering review

- Demonstrate the working feature.
- Explain the business or learning problem.
- Review architecture changes.
- Defend trade-offs.
- Discuss cost and operational impact.
- Identify what should not remain in production.

## Part B — Certification review

- Read each week’s official objectives aloud.
- Show evidence for every objective.
- Complete rapid scenario questions.
- Review the exam error log.
- Assign competency levels.
- Identify missing or weak coverage.
- Schedule remediation.

### Required review questions

For every service:

1. What official objective does this satisfy?
2. What problem does it solve?
3. What is the closest plausible alternative?
4. Why is that alternative wrong in this scenario?
5. What is one common configuration failure?
6. Where would I look first to troubleshoot it?
7. What part can I implement in Python?
8. Should this service remain in the final Zapas architecture?

---

## 17. Architecture Decision Record Discipline

Use ADRs for meaningful decisions, not every minor change.

Recommended structure:

```markdown
# ADR-NNN: Decision title

## Status
Proposed | Accepted | Superseded | Rejected

## Context
What problem or exam objective requires a decision?

## Options
What realistic alternatives were considered?

## Decision
What was selected?

## Exam relevance
Which AI-200 objective IDs are exercised?

## Consequences
Benefits, costs, risks, and operational effects.

## Validation
How will the decision be tested or revisited?
```

Minimum expected ADRs:

- Container image strategy
- Azure hosting platform
- Cosmos DB partitioning and consistency
- AI data-store selection
- Cache and vector retrieval
- Messaging and eventing
- Secrets, configuration, and observability

---

## 18. Engineering Wisdom Journal

`engineering_wisdom.md` captures lessons that outlive the exam.

Recommended prompts:

- What became simpler after I understood the actual requirement?
- What did I add only because the service was available?
- What service would I remove from production?
- What was harder to operate than to build?
- What failure did the happy-path tutorial hide?
- Which optimization mattered, and which was premature?
- Where did an exam distinction reveal a real architecture distinction?
- What did I change my mind about?
- Which decision would change at ten times the scale?
- How would I explain this to a junior developer, a senior engineer, and a product manager?

---

## 19. Cost and Environment Discipline

The residency must avoid uncontrolled Azure spending.

### Rules

- Configure a budget and alerts before major deployments.
- Tag resources by week and purpose.
- Use disposable resource groups for focused labs.
- Delete AKS and other expensive temporary resources after their labs.
- Stop or scale down resources when practical.
- Avoid retaining duplicate databases after comparison work is complete.
- Record cleanup steps in every weekly playbook.
- Never leave a service running merely because provisioning it was difficult.

### Suggested resource-group names

```text
zapas-w01-container-lab
zapas-w02-aks-lab
zapas-w03-cosmos-lab
zapas-w04-postgres-lab
zapas-w05-redis-lab
zapas-w06-integration-lab
zapas-w07-operations-lab
zapas-capstone
```

### Cleanup evidence

Maintain `cost_and_cleanup_log.md` with:

| Resource group | Purpose | Created | Estimated cost risk | Deleted or scaled down | Notes |
|---|---|---|---|---|---|

---

## 20. Rules of the Residency

1. The official AI-200 study guide is the source of truth.
2. Every current official objective must have practical evidence.
3. Zapas remains the primary learning product.
4. Focused labs are allowed when permanent integration would damage the architecture.
5. Python is practiced every week.
6. App Service, Container Apps, and AKS are all covered because all are explicitly assessed.
7. Exam-core topics take priority over career-extension topics.
8. Microsoft Foundry, Semantic Kernel, and Azure AI Search cannot replace missing AI-200 objectives.
9. Every week includes exam questions and active recall.
10. Every week includes troubleshooting, not only deployment.
11. Every incorrect answer becomes a reusable lesson.
12. Every architectural service must earn its place.
13. Measure before optimizing.
14. Simplicity wins until evidence demands complexity.
15. A successful deployment is not proof of understanding.
16. If the resident cannot explain why alternatives are wrong, the objective is not mastered.
17. The final architecture should be coherent, not a catalog of Azure services.
18. The certification is a formal graduation requirement, not an optional side effect.
19. No week is complete while an assigned objective remains unassessed.
20. The exam outline is revalidated before the exam is scheduled.

Every lab and implementation must be reproducible from the written playbook alone, using complete step-by-step instructions, full code, explicit commands, validation checkpoints, troubleshooting guidance, and cleanup steps.

---

## 21. Living Documents

Maintain throughout the residency:

- `exam_objective_matrix.md` — objective progress, level, and evidence
- `exam_error_log.md` — incorrect answers and corrected mental models
- `exam_notes.md` — concise notes organized by official domain
- `engineering_wisdom.md` — durable engineering lessons
- `architecture_evolution.md` — diagrams and architecture changes
- `decision_log.md` — smaller decisions that do not require full ADRs
- `tech_lead_reviews.md` — weekly review outcomes
- `interview_journal.md` — explanations and senior-level communication practice
- `cost_and_cleanup_log.md` — resources, cost observations, and deletion status

---

## 22. Weekly Playbooks

This blueprint defines the stable program. Detailed execution belongs in:

- `week01_container_images_and_app_service.md`
- `week02_container_apps_keda_and_aks.md`
- `week03_cosmos_db_and_vector_data.md`
- `week04_postgresql_pgvector_and_rag.md`
- `week05_redis_caching_and_vector_indexing.md`
- `week06_service_bus_event_grid_and_functions.md`
- `week07_security_observability_and_kql.md`
- `week08_capstone_and_exam_readiness.md`

Each weekly playbook must contain:

- Exact official objectives
- Required Microsoft Learn and documentation references
- Daily plan
- Zapas implementation tasks
- Python exam-fluency lab
- Commands and configuration checkpoints
- Failure-injection exercises
- Exam-style questions
- Tech Lead review prompts
- Deliverables
- Cost cleanup
- Competency gate

The blueprint should remain stable. Weekly playbooks may evolve as Microsoft documentation, SDKs, or the official outline change.

### Implementation detail standard
Every hands-on activity—including Zapas implementations, focused labs, Python labs, Azure CLI exercises, Azure portal exercises, Docker commands, Kubernetes manifests, configuration tasks, and deployment procedures—must be presented as a complete step-by-step implementation.

Each implementation must include:

- Required prerequisites and starting state
- Files, projects, folders, packages, and Azure resources to create
- Complete commands without omitted intermediate steps
- Full code and configuration required for the working implementation
- The exact file and location where each code or configuration change belongs
- Expected command output or observable result at important checkpoints
- Instructions for validating that the implementation works
- Common errors and troubleshooting guidance
- Cleanup commands and resource-removal steps when applicable

Instructions must not rely on phrases such as “configure the service,” “deploy the application,” “add the necessary code,” or “complete the setup” without explaining exactly how to perform those actions.

Azure exercises should provide the Azure CLI path by default. When using the Azure portal provides meaningful exam preparation, the portal procedure should also be explained step by step. Where both approaches are relevant, the playbook should clearly distinguish them.

---

## 23. Graduation Criteria

The residency has two graduation dimensions.

## Certification readiness

The resident must:

- Cover every current AI-200 objective.
- Reach Level 3 or higher on every objective.
- Reach Level 4 on high-risk objectives.
- Complete at least two full timed mock assessments.
- Maintain an exam error log and correct recurring mistakes.
- Recheck the official study guide before scheduling.
- Read and modify Python SDK examples.
- Demonstrate representative hands-on tasks from every exam domain.
- Use the official exam sandbox before exam day.

## Senior engineering readiness

The resident must:

- Present the Zapas architecture clearly.
- Explain why each retained service exists.
- Explain why rejected services were not retained.
- Compare alternatives without making absolute claims.
- Diagnose realistic failures.
- Discuss cost, scale, security, and operations.
- Defend ADRs under questioning.
- Communicate decisions concisely.
- Leave the repository in a maintainable state.

### Final graduation statement

The resident should be able to say:

> “I prepared for AI-200 by implementing every measured skill in a real Azure engineering context. I can deploy and troubleshoot containerized applications, work with the data services used by AI workloads, build event-driven and serverless integrations, secure configuration, trace distributed systems, query telemetry with KQL, and explain the trade-offs behind the architecture. The certification validates the work, while the repository demonstrates it.”

---

## 24. Post-Certification Extension

The following subjects remain valuable for the broader Senior Azure AI Engineer path, but they should be expanded after the AI-200 core is secure:

- Microsoft Foundry
- Azure OpenAI
- Azure AI Search
- Semantic Kernel
- Agent orchestration
- Prompt Flow
- Model evaluation
- Responsible AI
- Content safety
- Advanced RAG
- Agentic systems
- Infrastructure as code
- Advanced CI/CD
- Production cost governance

These can become a second residency or an extension block. They must not displace current AI-200 exam objectives during the eight-week certification program.

---

## 25. Final Principle

> **The residency does not choose between building and certification preparation. It uses building to master the certification objectives, and it uses the certification outline to prevent the build from becoming directionless.**
