use anyhow::Result;
use serde::Deserialize;
use std::fs;
use std::path::Path;

#[derive(Debug, Deserialize)]
pub struct Config {
    #[serde(default)]
    pub analyzers: AnalyzersConfig,
}

#[derive(Debug, Default, Deserialize)]
pub struct AnalyzersConfig {
    #[serde(default = "default_true")]
    pub todo_cheker: bool,

    #[serde(default = "default_true")]
    pub long_line_checker: bool,

    #[serde(default = "default_true")]
    pub god_class_checker: bool,

    #[serde(default = "default_max_line_length")]
    pub max_line_length: usize,

    #[serde(default = "default_max_class_lines")]
    pub max_class_lines: usize,
}

fn default_true() -> bool {
    true
}

fn default_max_line_length() -> usize {
    100
}

fn default_max_class_lines() -> usize {
    500
}

impl Default for Config {
    fn default() -> Self {
        Config {
            analyzers: AnalyzersConfig {
                todo_cheker: true,
                long_line_checker: true,
                god_class_checker: true,
                max_line_length: 100,
                max_class_lines: 500,
            },
        }
    }
}

impl Config {
    pub fn load(path: &Path) -> Result<Self> {
        let content = fs::read_to_string(path)?;
        let config: Config = toml::from_str(&content)?;

        Ok(config)
    }
}
