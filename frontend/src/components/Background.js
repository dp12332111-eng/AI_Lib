import React, { useState } from 'react';

const Background = () => {
    const [particles] = useState(() => Array.from({ length: 15 }).map((_, i) => ({
        id: i,
        left: `${Math.random() * 100}%`,
        delay: `${Math.random() * 5}s`,
        duration: `${15 + Math.random() * 10}s`,
        size: `${Math.random() * 20 + 5}px`,
    })));

    return (
        <>
            <div className="bg-animation" />
            <div className="particles">
                {particles.map((p) => (
                    <div
                        key={p.id}
                        className="particle"
                        style={{
                            left: p.left,
                            width: p.size,
                            height: p.size,
                            animationDelay: p.delay,
                            animationDuration: p.duration,
                        }}
                    />
                ))}
            </div>
        </>
    );
};

export default Background;
