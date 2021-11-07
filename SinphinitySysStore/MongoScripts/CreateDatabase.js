db.createCollection("styles", {
	validator: {
		$jsonSchema: {
			bsonType: "object",
			required: [ "name" ],
			properties: {
				name: {
				   bsonType: "string",
				   description: "must be a string and is required"
				}
			}
		}
	}
})

db.styles.insertOne({name: "Classic"})
db.styles.insertOne({name: "Rock"})
db.styles.insertOne({name: "Jazz"})

db.createCollection("bands", {
   validator: {
		$jsonSchema: {
			bsonType: "object",
			required: [ "name", "style" ],
			properties: {
				name: {
				   bsonType: "string",
				   description: "must be a string and is required"
				},
				style: {
					bsonType: "object",
					required: ["_id", "name"],
					properties:{
						_id: {
							bsonType: "objectId",
							description: "must be a string and is required"
							},
						name: {
							bsonType: "string",
							description: "must be a string and is required"
							}
					}
				}
			}
		}
	}
})


db.createCollection("songsInfo", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["name", "style", "band"],
            properties: {
                name: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                songDataId: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                songMidiId: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                isSongProcessed: {
                    bsonType: "bool",
                    description: "must be a bool"
                },
                isMidiCorrect: {
                    bsonType: "bool",
                    description: "must be a bool"
                },
                style: {
                    bsonType: "object",
                    required: ["_id", "name"],
                    properties: {
                        _id: {
                            bsonType: "objectId",
                            description: "must be a string and is required"
                        },
                        name: {
                            bsonType: "string",
                            description: "must be a string and is required"
                        }
                    }
                },
                band: {
                    bsonType: "object",
                    required: ["_id", "name"],
                    properties: {
                        _id: {
                            bsonType: "objectId",
                            description: "must be a string and is required"
                        },
                        name: {
                            bsonType: "string",
                            description: "must be a string and is required"
                        }
                    }
                },
                midiBase64Encoded: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                durationInSeconds: {
                    bsonType: "int",
                    description: "must be an integer"
                },
                durationInTicks: {
                    bsonType: "long",
                    description: "must be an integer"
                },
                averageTempo: {
                    bsonType: "int",
                    description: "must be an integer"
                },
                midiStats: {
                    bsonType: "object",
                    required: [],
                    properties: {
                        totalTracks: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalChannels: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalNoteEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalTempoChanges: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalPitchBendEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalProgramChangeEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalControlChangeEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalSustainPedalEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalChannelIndependentEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                    }
                }
            }
        }
    }
})

db.createCollection("songsMidi", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["midiBase64Encoded", "songInfoId"],
            properties: {
                songInfoId: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },   
                midiBase64Encoded: {
                    bsonType: "string",
                    description: "must be a string and is required"
                } 
            }
        }
    }
})
db.createCollection("patterns", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["midiBase64Encoded", "songInfoId"],
            properties: {
                asString: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                numberOfNotes: {
                    bsonType: "int",
                    description: "must be an integer"
                },
                durationInTicks: {
                    bsonType: "int",
                    description: "must be an integer"
                }
            }
        }
    }
})
db.createCollection("patterns", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["asString", "numberOfNotes", "durationInTicks"],
            properties: {
                asString: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                numberOfNotes: {
                    bsonType: "int",
                    description: "must be an integer"
                },
                durationInTicks: {
                    bsonType: "int",
                    description: "must be an integer"
                }
            }
        }
    }
})


db.createCollection("patternOccurrences", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["patternId", "songInfoId", "voice", "barNumber", "beat"],
            properties: {
                patternId: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                songInfoId: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                voice: {
                    bsonType: "int",
                    description: "must be an integer"
                },
                barNumber: {
                    bsonType: "int",
                    description: "must be an integer"
                },
                beat: {
                    bsonType: "int",
                    description: "must be an integer"
                }
            }
        }
    }
})

db.createCollection("patternsSongs", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["patternId", "songInfoId", "patternAsString"],
            properties: {
                patternId: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                songInfoId: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },                
                patternAsString: {
                    bsonType: "string",
                    description: "must be a string and is required"
                }
            }
        }
    }
})

//db.createCollection("songsData", {
//    validator: {
//        $jsonSchema: {
//            bsonType: "object",
//            required: ["songInfoId", "bars", "simplifications"],
//            properties: {
//                songInfoId: {
//                    bsonType: "string",
//                    description: "must be a string and is required"
//                },       
//                bars: {
//                    bsonType: "array",
//                    uniqueItems: true,
//                    additionalProperties: false,
//                    items: {
//                        bsonType: "object",
//                        required: ["barNumber"],
//                        properties: {
//                            barNumber: {
//                                bsonType: "int",
//                                description: "must be an integer"
//                            },
//                            ticksFromBeginningOfSong: {
//                                bsonType: "int",
//                                description: "must be an integer"
//                            },
//                            tempoInMicrosecondsPerQuarterNote: {
//                                bsonType: "int",
//                                description: "must be an integer"
//                            },
//                            hasTriplets: {
//                                bsonType: "bool",
//                                description: "must be a bool"
//                            },
//                            timeSignature: {
//                                bsonType: "object",
//                                required: ["numerator", "denominator"],
//                                properties: {
//                                    numerator: {
//                                        bsonType: "int",
//                                        description: "must be an integer"

//                                    },
//                                    denominator: {
//                                        bsonType: "int",
//                                        description: "must be an integer"

//                                    }
//                                }
//                            },
//                            keySignature: {
//                                bsonType: "object",
//                                required: ["key", "scaleType"],
//                                properties: {
//                                    key: {
//                                        bsonType: "int",
//                                        description: "must be an integer"

//                                    },
//                                    scale: {
//                                        bsonType: "string",
//                                        description: "must be a string"

//                                    }
//                                }
//                            }
//                        }
//                    }
//                },
//                tempoChanges: {
//                    bsonType: "array",
//                    items: {
//                        bsonType: "object",
//                        required: ["microsecondsPerQuarterNote", "ticksSinceBeginningOfSong"],
//                        properties: {
//                            microsecondsPerQuarterNote: {
//                                bsonType: "long",
//                                description: "must be an integer"
//                            },
//                            ticksSinceBeginningOfSong: {
//                                bsonType: "long",
//                                description: "must be an integer"
//                            }
//                        }
//                    }
//                },
//                simplifications: {
//                    bsonType: "array",
//                    items: {
//                        bsonType: "object",
//                        properties: {
//                            version: {
//                                bsonType: "int",
//                                description: "must be an integer"
//                            },
//                            numberOfVoices: {
//                                bsonType: "int",
//                                description: "must be an integer"
//                            },
//                            notes: {
//                                bsonType: "array",
//                                items: {
//                                    bsonType: "object",
//                                    required: ["pitch", "volume", "startSinceBeginningOfSongInTicks", "endSinceBeginningOfSongInTicks", "voice"],
//                                    properties: {
//                                        guid: {
//                                            bsonType: "string",
//                                            description: "must be a string"
//                                        },
//                                        pitch: {
//                                            bsonType: "int",
//                                            description: "must be an integer"
//                                        },
//                                        volume: {
//                                            bsonType: "int",
//                                            description: "must be an integer"
//                                        },
//                                        startSinceBeginningOfSongInTicks: {
//                                            bsonType: "long",
//                                            description: "must be an integer"
//                                        },
//                                        endSinceBeginningOfSongInTicks: {
//                                            bsonType: "long",
//                                            description: "must be an integer"
//                                        },
//                                        voice: {
//                                            bsonType: "int",
//                                            description: "must be an integer"
//                                        },
//                                        instrument: {
//                                            bsonType: "int",
//                                            description: "must be an integer"
//                                        },
//                                        isPercussion: {
//                                            bsonType: "bool",
//                                            description: "must be a boolean"
//                                        },
//                                        pitchBending: {
//                                            bsonType: "array",
//                                            items: {
//                                                bsonType: "object",
//                                                properties: {
//                                                    ticksSinceBeginningOfSong: {
//                                                        bsonType: "long",
//                                                        description: "must be a long"
//                                                    },
//                                                    pitch: {
//                                                        bsonType: "int",
//                                                        description: "must be an int"
//                                                    }
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                },
//               }
//        }
//    }
//})




let stylito = db.styles.findOne({ name: "Classic" })

db.bands.insertOne({ name: "John Sebastian Bach", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Wolfgang Amadeus Mozart", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Ludwig van Beethoven", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Frederic Chopin", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Antonio Vivaldi", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "George Frideric Handel", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Joseph Haydn", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Franz Schubert", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Franz Liszt", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Johannes Brahms", style: { _id: stylito._id, name: stylito.name } })



stylito = db.styles.findOne({ name: "Jazz" })

db.bands.insertOne({ name: "Chick Corea", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Herbie Hancock", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Weather Report", style: { _id: stylito._id, name: stylito.name } })


stylito = db.styles.findOne({ name: "Rock" })

db.bands.insertOne({ name: "AC DC", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Aerosmith", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Aha", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Al Stewart", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Alan Parsons", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Alice Cooper", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Alphaville", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "America", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Asia", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Beach Boys", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Beatles", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Bee Gees", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Black Sabbath", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Boney M", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Boston", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Bread", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Bruce Hornsby", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Bryan Adams", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Cat Stevens", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Chicago", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Creedence Clearwater Revival", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Counting Crows", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Crosby Stills Nash", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Deep Purple", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Def Leppard", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Depeche Mode", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Dire Straits", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Doobie Brothers", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Doors", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Duran Duran", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Eagle-Eye Cherry", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Eagles", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Edgar Winter", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Electric Light Orchestra", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Elton John", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Emerson Lake Palmer", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Enigma", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Europe", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Focus", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Foreigner", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Frankie Goes To Hollywood", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Free", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Genesis", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "George Harrison", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Gerry Rafferty", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Golden Earring", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Grateful Dead", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Guns and Roses", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Irene Cara", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "James Taylor", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Jean Luc Ponty", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Jethro Tull", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Jimmy Hendrix", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Joan Jett", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Joe Cocker", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Joe Walsh", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "John Lennon", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "John Miles", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Kansas", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "KC and the Sunshine Band", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Led Zeppelin", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Lenny Kravitz", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Manfred Mann", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Michael Jackson", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Midnight Oil", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Mike and the Mechanics", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Mike Oldfield", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Nirvana", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Oasis", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Pet Shop Boys", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Peter Frampton", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Peter Gabriel", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Phil Collins", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Pink Floyd", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Police", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Queen", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Rainbow", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "REM", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Reo Speedwagon", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Rick Wakeman", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Roberta Flack", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Rolling Stones", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Roxette", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Rush", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Santana", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Sash", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Scorpions", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Seal", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Simply Red", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Sting", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Styx", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Supertramp", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Survivor", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Tears for Fears", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Connells", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Cult", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Guess Who", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Kinks", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Knack", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Mamas and the Papas", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Outfield", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Smiths", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "The Verve", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Thin Lizzy", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Tom Petty", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Toto", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "U2", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "UB40", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Ugly Kid Joe", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Van Halen", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Village People", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Whitesnake", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Yes", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "ZZ Top", style: { _id: stylito._id, name: stylito.name } })
