import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './Chatbot.css';
import userIcon from '../../assets/user.png';
import botIcon from '../../assets/bot.png';
import ziggyIcon from '../../assets/ziggy.png';
import { v4 as uuidv4 } from 'uuid'; // For generating session IDs

const Chatbot = () => {
    const [isOpen, setIsOpen] = useState(false); // Track if chatbot is open
    const [messages, setMessages] = useState([]);
    const [userInput, setUserInput] = useState('');
    const [sessionId, setSessionId] = useState(''); // Session ID for chat context

    // Generate or retrieve a session ID when the chatbot loads
    useEffect(() => {
        const storedSessionId = localStorage.getItem('sessionId') || uuidv4();
        setSessionId(storedSessionId);
        localStorage.setItem('sessionId', storedSessionId);
    }, []);

    const handleToggle = () => {
        setIsOpen(!isOpen);

        if (!isOpen && messages.length === 0) {
            setMessages([{ role: 'bot', content: "Hi there! I'm Ziggy, your personal assistant. How can I help you today?" }]);
        }
    };

    const sendMessage = async () => {
        if (!userInput.trim()) return;

        const newMessages = [...messages, { role: 'user', content: userInput }];
        setMessages(newMessages);

        try {
            const response = await axios.post('http://localhost:4000/api/chat', {
                message: userInput,
                sessionId, // Send session ID with each request
            });

            setMessages([...newMessages, { role: 'bot', content: response.data.reply }]);
        } catch (error) {
            console.error('Error:', error);
            setMessages([...newMessages, { role: 'bot', content: "Sorry, I'm having trouble responding right now." }]);
        }

        setUserInput('');
    };

    return (
        <div className="chatbot-container">
            {/* Tab Button */}
            <div className="chatbot-tab" onClick={handleToggle}>
                <img src={ziggyIcon} alt="Ziggy" className="ziggy-icon" />
                <span>Ziggy</span>
            </div>

            {/* Chatbot Window */}
            {isOpen && (
                <div className="chatbot">
                    <div className="chat-window">
                        {messages.map((msg, index) => (
                            <div key={index} className={msg.role === 'user' ? 'user-message' : 'bot-message'}>
                                {msg.role === 'user' ? (
                                    <>
                                        <span>{msg.content}</span>
                                        <img src={userIcon} alt="User" />
                                    </>
                                ) : (
                                    <>
                                        <img src={botIcon} alt="Bot" />
                                        <span>{msg.content}</span>
                                    </>
                                )}
                            </div>
                        ))}
                    </div>
                    <div className="chat-input">
                        <input
                            type="text"
                            value={userInput}
                            onChange={(e) => setUserInput(e.target.value)}
                            onKeyPress={(e) => e.key === 'Enter' && sendMessage()}
                            placeholder="Type your message..."
                        />
                        <button onClick={sendMessage}>Send</button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Chatbot;
