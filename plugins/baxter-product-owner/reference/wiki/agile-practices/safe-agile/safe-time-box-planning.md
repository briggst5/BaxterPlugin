# Agile Practices/SAFe Agile/SAFe Time Box Planning

[[_TOC_]]


# Time Box Definitions
The Scaled Agile Framework (SAFe) uses timebox scheduling to deliver continuous value and alignment with project objectives.  This document defines the timeboxes and their durations used here.

|Interval| Duration  |
|--|--|
| Planning Interval | One Quarter (6 iterations) |
| Iteration | One Half Month (15 days) |

## Planning Interval

A Planning Interval (PI) is a fixed timebox for planning, building, validating, and delivering the project goals.  Depending on the PI goals, by the end of the PI, the team delivers artifacts such as documents, hardware, or software.  The PI duration chosen here is one quarter (three months).

## Iteration

An iteration is a smaller, fixed timebox.  Depending on the iteration goals, the team is delivering some complete artifacts or achieving tangible progress towards completing others.  The iteration is assigned to a work item using the `Iteration Path` field in a work item:

![sprint.png](/.attachments/sprint-7da22bdb-9416-4fcd-9375-7fdc7d98f912.png =200x)

## Configuring Iterations

1. To configure iterations, including add/remove, or set dates, open project settings:
![iters.png](/.attachments/iters-3ba41543-ca7a-4302-863e-899e25cb0cf6.png =150x)

1. Use the editor as needed:
![editor.png](/.attachments/editor-045c93e0-6c07-457f-b129-a890fdf551e9.png =200x)

# Milestone

A defined stage of development, typically corresponding to completing of Epics and Features, and delivery of some major asset(s).  It represents a major achievement for the team and is important to establish the sense of progress that the team has made.

# Work

Agile work planning and estimating is an iterative approach to project management that emphasizes collaboration and prioritizes goals like customer satisfaction and delivering high-quality results over time keeping.  Agile teams use the idea of "effort" rather than hours to estimate the time required to complete tasks.  Studies have shown that estimating in hours is almost never accurate anyway and really just contributes to individual stress which has detrimental effects on achieving the project goals.

Effort is an abstract and flexible unit of measurement that account for the unpredictable factors that impact the time actually required to complete a task.  These factors can include:

* complexity of the work,
* unknowable unknowns,
* non-project demands,
* skill level of team members, and
* dependency on other resources.

## Story Points 
One of the most common measures of effort are "story points."  These points are a unit-less measure of the overall "effort" needed to complete a task.  Using story points, tasks that are complex with many unknowns will have a high effort and may need to be decomposed into smaller tasks with more concrete objectives.  

One of the most common ways to assign effort follows the Fibonacci sequence.  The sequence allows fast, exponential growth, but is easy to generate the next number, which is the sum of the first two numbers.   Following are the first 10 Fibonacci numbers:

1, 1, 2, 3, 5, 8, 13, 21, 34, 55

## Individual Capacity
The capacity of an individual team member can vary depending on their role, skill level, experience, and commitments outside the team.  As a general rule, the total story points an individual can complete in a given two-week iteration is between 3 and 9 story points, with an average of 5 story points per iteration.  This is absolutely a rule of thumb, and there will be significant variance.

It is absolutely critical that the goal for using story points is not compare two individuals' capacity.  Under no circumstances should capacity be used as a performance measurement.  If the goal is to deliver quality and customer delight; and that depends on the ability to accurately estimate the effort, then weaponizing capacity will almost certainly kill off the teams' ability to execute and make those goals unreachable.

## Team Capacity

The sum of the individuals' capacity in a given iteration is used to compute the team's overall capacity.  As individuals should not schedule more than their individual capacity, the team should not plan on more effort than the team's total capacity.  

## Decomposition

If an item has more than an individual's capacity, then that indicates that the task must be decomposed into smaller and smaller tasks, until the overall effort fits within an iteration.  For example, suppose we have a new feature request to determine whether an image shows a cancerous lesion.  This clearly has a huge degree of uncertainty and complexity.  Suppose the team assigns it a full 65 points (and even that may be too low).  We need to start breaking down the feature into smaller and smaller pieces:

::: mermaid
 graph LR;
 A[Detect Cancer Lesion in Image - 65] --> B[Capture Image - 21];
 A --> C[Pre-Process Image - 13];
 A --> D[Pass into classifier - 3];
 A --> E[Report classifier results - 3];
:::

There are some important points to take note of.  First, when the overall project was assessed, it had a huge number of story points, which makes sense.  There were many unknowns, and the work wasn't broken down any further.  When the task was selected and broken down further, the team can focus on getting the right individuals involved.  For example, someone who has never worked with machine learning models will certainly require more effort than an expert who performs that task every day.  The goal is to use this information to retire risk, learn more about the requirements, and better define the work.

Second, there are tasks that still need to be refined further, but they do not need to be refined now.  Splitting tasks promotes better independence between task steps which leads to better testing and validation.  

## Definition of Done
Finally, each one of these tasks will have a more refined statement of what done looks like.  This is known as the `Definition of Done`, and is crucial to plan and execute the task.  During planning, the team agrees upon the definition of done and uses that a guide of when the task is done or not.  This Definition of Done forms the contract for what will be delivered.  The focus must be on that definition and only that definition.  

Imagine hiring a plumber to fix a pipe in your home.  While there, the plumber also sees an old pipe that maybe should be replaced soon, a leaking hose faucet outside the home, and a loose electrical wall plate.  If the plumber stops to fix everything, then all of the other customers waiting for their critical issues will be left wet and angry.  The customer may also be upset when they get the bill charging them for extra services that they didn't expect to pay for.  

Engineers, by nature, want to solve the problems that they see in front of them.  This definition of done must serve as a guide to keep focused on the urgent problem right now.  We will make note of the leaking faucets and loose wires and schedule them in a future sprint.
 
An individual should never pick a task for which they have outstanding questions about their expectations.  This can be highly individual specific.  The level of clarity for an Engineer 1 vs. a Senior Engineer are likely very different.  

## Estimating Effort
A very mature, well-functioning Agile team with empowered individuals will likely be able to schedule themselves.  However, the vast majority of teams and especially those who are new to Agile and task estimation will benefit from a group effort to estimate effort.  There are strategies, such as _Planning Poker_ that can assist with this, but in general, during a planning session, the team collectively works through the story point estimates for items.  This is an important exercise, as everyone can get visibility into what makes one task more or less complex.

The team will also work together to ensure that the definition of done is concrete and well understood, and then the team will discuss the effort that will be required.  The team can split large tasks apart or can wait to do that in the future.  This is a good example of the iterative nature of Agile planning.

## Tracking Effort
All DevOps work items have an Effort Tracker:

![effort.png](/.attachments/effort-4defcc26-f9af-4d34-b2b3-f64cbdd52627.png)


During the planning meetings, the team will fill in the agreed upon effort.  

While the work progresses, individuals working on the item will update the remaining and completed work effort.  Again, the goal is not to make a complicated time clock, but to communicate progress.  One of the common complaints of engineers is that they must spend their time answering the question "are you done yet?"  By using Agile tools like this, project managers can just read the board and answer the question themselves.

## Reflecting on Estimations
It is absolutely critical that everyone agrees that work estimates are just that - estimates.  They are not legally binding contracts.  Getting an estimate wrong is to be expected.  Getting an estimate **very wrong** happens all the time.  One of the most important and valuable steps in the Agile process is reflecting on the estimates and reality.  At the end of a sprint, individuals should compare their estimates against reality and use that information to reflect on what went wrong.  What factors weren't predicted?  Could they be in the future?  

Ultimately, the goal is to become more accurate and deterministic.  Unless individuals enjoy working nights and weekends to hit sprint goals, getting a more accurate idea of estimates and capacity are crucial to learning what you are capable of.
