<!-- b131a2eb-d94c-4244-bd5c-8dc5ed4709a6 e76cf5e2-5efe-4599-a852-844c10f7571e -->
# Fix Chatbot to Route All Messages to C# Recommendation Service

## Problem

The chatbot only routes messages containing "recommend" or "suggest" keywords to the C# recommendation service. Other queries (like "I want vegetarian from the menu") fall through to generic GPT responses without menu context.

## Solution

Simplify the chat controller to send ALL user messages to the C# recommendation service, letting it intelligently handle all food-related and general queries. Use a simple fallback message if the service is unavailable.

## Changes

### backend/controllers/chatController.js

1. Remove keyword-based routing logic (lines 36-46 for FAQ, lines 48-107 for recommendation keywords)
2. Keep only the C# service call as the primary handler
3. Simplify fallback: remove GPT fallback, show simple error message
4. Remove the default OpenAI API section (lines 147-181) since C# handles everything
5. Keep conversation history tracking for session continuity

**Flow:**

- FAQ responses → Remove (C# service will handle)
- All messages → C# recommendation service
- If C# service fails → "Something went wrong, please try again"
- No direct OpenAI calls from Node.js

## Files to Modify

- `backend/controllers/chatController.js` - Simplify routing logic to always use C# service

### To-dos

- [ ] Remove FAQ handling, keyword matching, and default GPT routing from chatController.js
- [ ] Make C# recommendation service the sole handler for all chat messages
- [ ] Replace complex fallback with simple error message when C# service unavailable
- [ ] Test chatbot with various queries to verify C# service routing works