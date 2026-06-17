# Training and Development/Azure DevOps Excel Plugin (Office Integration)

# Using the Azure DevOps Excel Plugin (Office Integration)

This page shows **why and when to use the Excel plugin**, how to **install** it, and a **step‑by‑step example** to connect Excel to Azure DevOps and publish changes from a query. It also includes **troubleshooting** and **best practices**.

---
 

## Why use the Excel plugin?

  

**Common use cases**

- **Bulk edit work items**: update fields (e.g., Area, Iteration, Assigned To, Story Points, custom fields) across dozens or hundreds of items in seconds.

- **Backlog hygiene**: sort, filter, and normalize data quickly during planning or PI preparation.

- **Data analysis**: leverage Excel’s filters, formulas, conditional formatting, PivotTables, and charts to spot trends and gaps.

- **Import/export**: stage new work items in Excel (e.g., migration from a legacy list) and then publish to Azure DevOps.

- **Non‑technical stakeholder review**: let business partners propose edits in a familiar tool before changes are published.

  

---

  

## Prerequisites

  

- **Windows + Microsoft Excel (desktop)**. The add‑in is not available on Mac Excel or Excel Online.

- **Access to your Azure DevOps organization and project** (read/update permissions on work items).

- Your org URL (e.g., `https://dev.azure.com/FLC-NPD`).

  

---

  

## Install the Azure DevOps Excel Plugin

  

> **Name you’ll see:** *Team Foundation Add‑in* (appears on the **Team** tab in Excel).

  

1. **Close Excel** if it’s open.

2. **Download & Install**

   - Get the **Azure DevOps Office Integration** installer used for the Excel add‑in (often included with Visual Studio/Team Explorer or provided by your IT software portal).

   - Run the installer and complete setup.

![==image_0==.png](/.attachments/==image_0==-4855daf9-2dee-455e-8543-de53f57917c9.png) 

3. **Verify in Excel**

   - Open **Excel** → you should see a **Team** tab on the ribbon.

   - If you don’t see it: **File → Options → Add‑ins** → at the bottom choose **COM Add‑ins** → **Go…** → ensure **Team Foundation Add‑in** is checked → **OK**.

  

> If your organization centrally manages software, you may find the installer in Company Portal/Software Center. If not, contact IT for the approved package name and version.

  

---

  

## Quick Start Example: Link a Query, Edit, and Publish

  

Follow this walkthrough to get hands‑on with the plugin.

  

### Step 1 — Connect from Excel

1. Open **Excel** → go to the **Team** tab.

2. Click **New List**.

3. Sign in with your **work account** when prompted.

4. Select your **Organization** and **Project**.

  

### Step 2 — Import a Shared Query

1. In the **New List** dialog, choose **Queries** in the left panel.

2. Browse to **Shared Queries** (example path): 

   `Shared Queries → Current Iteration → All Active Work Items`

3. Select the query → **OK**. 

   Excel will load a table with the query results (columns = work item fields).

  

> **Tip:** Flat list queries import cleanly. Tree and direct links queries also work; you can add related levels via **Team → Add Tree Level** later.

  

### Step 3 — Make Edits in Excel

- Update cells as needed: **Title**, **Assigned To**, **State**, **Iteration**, **Area**, **Priority**, **Story Points**, etc.

- Use Excel features: **filters**, **find/replace**, **fill down**, formulas for temporary calculations.

- **Add new work items** by inserting a new row at the bottom and filling required fields (e.g., *Work Item Type*, *Title*).

  

### Step 4 — Publish Changes to Azure DevOps

1. Click **Publish** on the **Team** tab.

2. Review the status pane; fix any validation issues flagged.

3. New items will receive **IDs** after publish.

4. Verify in **Boards → Work Items** or **Boards → Backlogs**.

  

---

  

## Mini Walkthrough: Bulk‑Set Priority in the Current Sprint

  

**Goal:** Set **Priority = 1** for all *Active* PBIs in the current iteration.

  

1. Excel → **Team → New List** → select your project.

2. Choose query: `Shared Queries → Sprint <N> → Active PBIs`.

3. Filter the **State** column to **Active** (should already be, but verify).

4. Set **Priority** column to **1** for all rows (use fill down).

5. Click **Publish**.

6. Confirm in **Boards → Backlogs** that items show **Priority = 1**.

  

---

  

## Best Practices

  

- **Use queries designed for Excel**: start with a *Flat list* for bulk edits. Switch to *Tree* if you need parent/child context and then **Add Tree Level** in Excel.

- **Add only needed columns**: fewer columns = faster imports and clearer edits. You can add/remove columns in the Excel list or adjust the query fields.

- **Do not rely on deleting rows** in Excel to remove items from Azure DevOps. Deleting a row just removes it from the sheet; it does **not** delete the work item in ADO.

- **Permission checks**: ensure you (or the service connection) have permission to edit the fields you’re changing (e.g., Area/Iteration paths).

- **Autosave**: if working from OneDrive/SharePoint, consider toggling Autosave off during large bulk updates to avoid partial drafts being saved mid‑edit.

  

---

  

## Troubleshooting

  

| Symptom | What to try |
|---|---|
| **No “Team” tab in Excel** | **File → Options → Add‑ins** → choose **COM Add‑ins** → **Go…** → enable **Team Foundation Add‑in**. If missing, reinstall the Office Integration. |
| **Can’t sign in / org not found** | Confirm you’re using your **work account** and the correct org URL. Clear cached credentials (Windows Credential Manager) and sign in again. |
| **Query loads but fields are missing** | Add columns in Excel (**Team → Choose Columns**) or update the query to include additional fields. |
| **Publish fails with validation errors** | Fix required fields highlighted in Excel, confirm valid **Area/Iteration** paths, and ensure you have edit permissions. |
| **Add‑in disabled after crash** | Excel may auto‑disable. Re‑enable in **File → Options → Add‑ins** (Manage: Disabled Items). |
 

---
 

## FAQ

  

**Q: Can I use the Excel plugin on a Mac?** 

A: No—the add‑in is supported only on **Windows desktop Excel**.

  

**Q: Can I import parent/child structures?** 

A: Yes. Use a **Tree query** and then **Team → Add Tree Level** to bring in related levels (e.g., PBI → Task).

  

**Q: Will formulas publish?** 

A: Formulas themselves do not publish; the **cell values** do. Use formulas for staging, then paste values where needed.

  

**Q: Does deleting a row delete the work item?** 

A: No. It only removes that row from your worksheet. Deletions must be done in Azure DevOps (or via REST/API).

  

---

  

## Appendix: Creating a Good “Excel‑Ready” Query

  

1. Go to **Boards → Queries** in Azure DevOps.

2. Click **New query** → *Flat list of work items*.

3. Add clauses (e.g., `Team Project = @Project`, `Work Item Type In [Product Backlog Item, Bug]`, `State <> Done`).

4. Add the fields you expect to edit: **Column options** → include **Title, Work Item Type, Assigned To, State, Area Path, Iteration Path, Priority, Story Points** (and custom fields).

5. Save it under **Shared Queries** (e.g., `Shared Queries → Excel → Active PBIs for Current Sprint`).

  

---

  

## Page Ownership

  

- **Owner:** _<Your name or team>_ 

- **When to update:** When the add‑in version changes, or if your process/fields change. 

- **Feedback:** Add comments on this page or create a work item (tag: `excel-plugin-doc`).

  

---
