# Project Management

## Overview

Manage mod projects: create, edit, delete, organize. Main window appears on application launch.

## Creating Projects

1. Click **New Project**
2. Fill required fields:
   - Project Name (no special chars)
   - Author
   - Version (default: 1.0.0)
   - Description (optional)
   - Workshop ID (optional)
3. Select Project Path
4. Click **Create**

## Editing Projects

- Double-click project row
- Click Edit button
- Opens Project Editor with 5 tabs:
  1. General - Project info
  2. ModConf - Config files
  3. ModImg - Images
  4. Variables - Global variables
  5. ModEvents - Event handlers

## Deleting Projects

1. Select project
2. Click Delete
3. Choose:
   - Yes: Delete project + folder
   - No: Remove from list only
   - Cancel: Abort

## Filtering

- Filter by Name, Description, or Author
- Multiple filters apply AND logic
- Clear filters to show all

## Workplace Management

- Browse: Select new workplace folder
- Each workplace has separate `projects.json`
- Can maintain multiple workplaces