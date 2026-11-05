# Decision-Making Questions

These answers are intentionally independent from the inventory solution. They describe how I make decisions under uncertainty, how I compare options and how I handle being wrong.

## 1. A recent technical decision made with incomplete information

**Situation.** I had to decide whether to keep an internal reporting endpoint on the primary database or move it to a read replica. The endpoint was slowing down checkout during month-end peaks. We did not have reliable numbers for three things: how much traffic the report would receive after a planned product launch, what latency the business could tolerate, and whether the finance team would accept data that lagged by a few seconds. The launch date was two weeks away and the replica infrastructure was owned by another team.

**What was missing and how I proceeded.** I separated what I knew from what I was assuming. I knew the current query plan, the peak write volume and the cost of the replica. I did not know future report volume or the tolerance for stale data. I wrote those assumptions down explicitly, with a range instead of a single number, and shared them with the product owner and the finance lead.

Then I chose the option with the smallest irreversible cost. Instead of committing to a replica, I added a short-lived cache and a query timeout to the endpoint, instrumented it, and set a threshold that would trigger the next step. That bought two weeks of real data with a one-day change. When the launch traffic arrived, the measurements showed the cache absorbed most of the load, and the replica was postponed rather than built on speculation.

**Why this worked.** The decision was not "cache versus replica". It was "what is the cheapest way to learn what we do not know before spending the expensive option". I time-boxed the temporary solution, defined the signal that would force a revisit, and made the assumptions visible so the team could challenge them.

**What I would do differently.** I would have asked finance about stale-data tolerance earlier. It turned out to be a non-issue, but it was the assumption with the highest potential to invalidate the whole approach, and I only confirmed it after the design was drafted.

## 2. Criteria for choosing between two equally valid approaches

When two options look equally valid on paper, I assume the tie is caused by missing context, not by true equivalence. I use a fixed set of criteria and make the weighting explicit.

**First, reversibility.** A decision that is cheap to undo can be made quickly with less information. A decision that is expensive to undo, such as a database engine choice or a public API contract, deserves a prototype and a longer review. I try to convert irreversible decisions into reversible ones, for example by isolating the choice behind an interface or a thin adapter.

**Second, operational cost.** I ask who will run this at 3 a.m. A solution that adds a component, a deployment pipeline or a new failure mode has a cost that does not appear in the design document. Fewer moving parts usually wins when the functional difference is small.

**Third, failure behavior.** I compare what happens when each option fails. A cache that fails open and a cache that fails closed produce very different user experiences. I prefer the option whose failure mode is contained and observable.

**Fourth, team fit.** The best design is the one the team can maintain, review and debug. Familiarity is not a reason to accept a bad design, but it is a real cost that should be priced in.

**Fifth, evidence.** If the options are still tied, I run a time-boxed spike against the real constraint, not a synthetic benchmark. The spike answers the question that matters and usually breaks the tie.

Finally, I record the decision and the criteria in an ADR or a short note. Six months later, the reasoning is more valuable than the choice itself. If the context changes, the note tells the next person which assumptions to revisit.

## 3. A decision I had to reverse, and what I learned

**Situation.** On a previous project I introduced a shared internal library to centralize logging, correlation ids and a few HTTP conventions across four services. The duplication was real and the first version worked well. I pushed it as the standard and asked the other teams to adopt it.

**What went wrong.** The library became a bottleneck. Every service needed a different release cadence, and a small change in the shared package forced coordinated deployments across teams that did not share a schedule. A bug in the correlation-id handling affected all four services at once. The library also started to absorb business concepts that did not belong there, because adding them was easier than designing them properly in each service. Within two months I had recreated a distributed monolith through a NuGet package.

**The reversal.** I proposed splitting the library. The truly cross-cutting pieces, correlation ids and log formatting, stayed in a small package with a strict scope and a versioning policy. The HTTP conventions and anything domain-specific moved back into the services. The migration took longer than the original consolidation, and I had to explain to two teams why the standard I had promoted was being undone.

**What I learned.**

1. **Duplication is cheaper than the wrong abstraction.** A shared library is a dependency with a social contract. If the contract is not stable, duplication gives teams the freedom to evolve.
2. **Centralization concentrates failure.** A shared component reduces per-service work but increases the blast radius. That trade-off has to be explicit.
3. **Adoption is a design constraint.** A library that requires coordinated releases across teams is not really decoupled, even if it is packaged as one.
4. **Reversing a decision is part of the process, not a failure.** What matters is detecting the problem early, communicating it honestly and limiting the cost of the correction.

Since then I apply a simple test before extracting shared code: has the same code stabilized in at least two places, and do the owners agree on its boundary? If the answer is no, I wait.
