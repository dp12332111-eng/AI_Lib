import React from 'react';
import { Github, Linkedin, MessageSquare, Facebook, MapPin, Mail, Phone } from 'lucide-react';

const Footer = () => {
    return (
        <footer className="footer bg-white border-t border-border pt-16 pb-10 px-4 relative z-10">
            <div className="container mx-auto max-w-7xl">
                <div className="grid md:grid-cols-2 gap-12 mb-12">
                    <div>
                        <h5 className="text-[0.95rem] font-bold mb-5 text-text-primary">Developer</h5>
                        <div className="developer-card">
                            <p className="font-bold text-text-primary mb-0.5">MP Online Group 30</p>
                            <p className="text-text-secondary text-[0.82rem] mb-4">AI/ML Engineers | Generative AI | Agentic AI</p>
                            <div className="flex flex-wrap gap-2.5">
                                <a href="https://github.com/dp12332111-eng/AI_Lib" target="_blank" className="inline-flex items-center gap-2 px-3 py-1.5 border border-border rounded-md text-text-secondary hover:bg-bg-hover hover:text-accent hover:border-accent transition-all text-xs font-medium decoration-0">
                                    <Github size={14} /> GitHub
                                </a>
                                
                                
                            </div>
                        </div>
                    </div>
                    <div>
                        <h5 className="text-[0.95rem] font-bold mb-5 text-text-primary">Contact</h5>
                        <ul className="list-none space-y-3.5">
                            <li className="flex items-center gap-3 text-text-secondary text-[0.88rem]">
                                <MapPin size={16} className="text-accent shrink-0" /> VIT Bhopal University
                            </li>
                        </ul>
                    </div>
                </div>
                
            </div>
        </footer>
    );
};

export default Footer;
