import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import {
  Card,
  Slider,
  Checkbox,
  Button,
  Row,
  Col,
  Image,
  Typography,
  message,
  Spin,
  Table,
} from 'antd';
import axios from 'axios';

const checksList = [
  'Brisque',
  'Blur',
  'Glare',
  'Exposure',
  'Contrast',
  'ColorDominance',
  'Noise',
  'MotionBlur',
  'Banding',
];

const defaultThresholds: Record<string, number> = {
  BrisqueMax: 50,
  BlurThreshold: 100,
  BrightThreshold: 240,
  AreaThreshold: 500,
  ExposureMin: 80,
  ExposureMax: 180,
  ContrastMin: 30,
  DominanceThreshold: 1.5,
  NoiseThreshold: 500,
  MotionBlurThreshold: 3,
  BandingThreshold: 0.5,
};

const sliderRanges: Record<string, [number, number]> = {
  BrisqueMax: [0, 100],
  BlurThreshold: [0, 300],
  BrightThreshold: [0, 255],
  AreaThreshold: [0, 2000],
  ExposureMin: [0, 255],
  ExposureMax: [0, 255],
  ContrastMin: [0, 100],
  DominanceThreshold: [0, 5],
  NoiseThreshold: [0, 1000],
  MotionBlurThreshold: [0, 10],
  BandingThreshold: [0, 5],
};

const sliderSteps: Record<string, number> = {
  DominanceThreshold: 0.1,
  BandingThreshold: 0.1,
  MotionBlurThreshold: 0.1,
};

const checkConfig: Record<string, any> = {
  Brisque: { valueKey: 'BrisqueScore', thresholdKey: 'BrisqueMax', type: '<=' },
  Blur: { valueKey: 'BlurScore', thresholdKey: 'BlurThreshold', type: '>=' },
  Glare: { valueKey: 'GlareArea', thresholdKey: 'AreaThreshold', type: '<=' },
  Exposure: {
    valueKey: 'Exposure',
    thresholdKey: ['ExposureMin', 'ExposureMax'],
    type: 'range',
  },
  Contrast: { valueKey: 'Contrast', thresholdKey: 'ContrastMin', type: '>=' },
  ColorDominance: {
    valueKey: 'ColorDominance',
    thresholdKey: 'DominanceThreshold',
    type: '<=',
  },
  Noise: { valueKey: 'Noise', thresholdKey: 'NoiseThreshold', type: '<=' },
  MotionBlur: {
    valueKey: 'MotionBlurScore',
    thresholdKey: 'MotionBlurThreshold',
    type: '<=',
  },
  Banding: { valueKey: 'BandingScore', thresholdKey: 'BandingThreshold', type: '<=' },
};

const QualityChecker: React.FC = () => {
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [result, setResult] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [generateHeatmaps, setGenerateHeatmaps] = useState(false);
  const [settings, setSettings] = useState<Record<string, number>>({ ...defaultThresholds });
  const [selectedChecks, setSelectedChecks] = useState<string[]>(checksList);

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const img = acceptedFiles[0];
    if (img) {
      setFile(img);
      setPreview(URL.createObjectURL(img));
      setResult(null);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop, accept: { 'image/*': [] } });

  const evaluateCheck = (check: string) => {
    if (!result) return null;
    const cfg = checkConfig[check];
    if (!cfg) return null;
    const value = result.Results?.[cfg.valueKey];
    if (value === undefined) return null;
    const curThr = Array.isArray(cfg.thresholdKey)
      ? cfg.thresholdKey.map((k: string) => settings[k])
      : settings[cfg.thresholdKey];
    const defThr = Array.isArray(cfg.thresholdKey)
      ? cfg.thresholdKey.map((k: string) => defaultThresholds[k])
      : defaultThresholds[cfg.thresholdKey];

    let pass = true;
    let diff = 0;
    if (cfg.type === '>=') {
      pass = value >= curThr;
      diff = Math.abs((value - curThr) / curThr) * 100;
    } else if (cfg.type === '<=') {
      pass = value <= curThr;
      diff = Math.abs((value - curThr) / curThr) * 100;
    } else if (cfg.type === 'range') {
      const [min, max] = curThr as number[];
      if (value < min) {
        pass = false;
        diff = ((min - value) / min) * 100;
      } else if (value > max) {
        pass = false;
        diff = ((value - max) / max) * 100;
      } else {
        const center = (min + max) / 2;
        diff = (Math.abs(value - center) / center) * 100;
      }
    }

    let color: 'green' | 'orange' | 'red' = 'green';
    if (!pass) {
      color = 'red';
    } else if (diff < 20) {
      color = 'orange';
    }

    return {
      check,
      value: value.toFixed ? value.toFixed(2) : value,
      threshold: Array.isArray(cfg.thresholdKey) ? curThr.join(' - ') : curThr,
      default: Array.isArray(cfg.thresholdKey)
        ? defThr.join(' - ')
        : defThr,
      diff: diff.toFixed(1) + '%',
      pass,
      color,
    };
  };

  const handleAnalyze = async () => {
    if (!file) {
      message.error("Carica un'immagine");
      return;
    }
    const formData = new FormData();
    formData.append('Image', file);
    selectedChecks.forEach(c => formData.append('Checks', c));
    Object.entries(settings).forEach(([k, v]) => {
      formData.append(`Settings.${k}`, String(v));
    });
    formData.append('Settings.GenerateHeatmaps', String(generateHeatmaps));
    try {
      setLoading(true);
      const resp = await axios.post('/quality/check', formData, { headers: { 'Content-Type': 'multipart/form-data' } });
      setResult(resp.data);
    } catch (err) {
      message.error('Errore durante il controllo');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card title="Image Quality Checker" style={{ maxWidth: 800, margin: 'auto' }}>
      <div {...getRootProps()} style={{ border: '2px dashed #aaa', padding: 20, textAlign: 'center', marginBottom: 20 }}>
        <input {...getInputProps()} />
        {isDragActive ? <p>Rilascia l\'immagine qui ...</p> : <p>Trascina un\'immagine o clicca per selezionare</p>}
      </div>

      {preview && <Image src={preview} alt="preview" style={{ marginBottom: 20 }} />}

      <Row gutter={16} style={{ marginBottom: 20 }}>
        <Col span={12}>
          <Slider
            min={0}
            max={300}
            value={settings.BlurThreshold}
            onChange={(v: number) =>
              setSettings(prev => ({ ...prev, BlurThreshold: v }))
            }
            marks={{ 0: '0', 100: '100', 200: '200', 300: '300' }}
          />
          <Typography.Text>Soglia Blur: {settings.BlurThreshold}</Typography.Text>
        </Col>
        <Col span={12}>
          <Checkbox checked={generateHeatmaps} onChange={e => setGenerateHeatmaps(e.target.checked)}>Genera heatmaps</Checkbox>
        </Col>
      </Row>
      <Row gutter={[16, 16]} style={{ marginBottom: 20 }}>
        {Object.entries(settings).map(([k, v]) =>
          k === 'BlurThreshold' ? null : (
            <Col span={12} key={k}>
              <Typography.Text>
                {k}: {v}
                {v !== defaultThresholds[k] && ` (default ${defaultThresholds[k]})`}
              </Typography.Text>
              <Slider
                min={sliderRanges[k][0]}
                max={sliderRanges[k][1]}
                step={sliderSteps[k] || 1}
                value={v}
                onChange={(val: number) =>
                  setSettings(prev => ({ ...prev, [k]: val as number }))
                }
              />
            </Col>
          )
        )}
      </Row>
      <Checkbox.Group
        options={checksList}
        value={selectedChecks}
        onChange={(vals: any) => setSelectedChecks(vals as string[])}
        style={{ marginBottom: 20 }}
      />
      <div style={{ textAlign: 'center' }}>
        <Button type="primary" onClick={handleAnalyze}>Analizza</Button>
      </div>

      {loading && <Spin style={{ display: 'block', marginTop: 20 }} />}

      {result && (
        <div style={{ marginTop: 20 }}>
          <Table
            dataSource={selectedChecks
              .map(c => evaluateCheck(c))
              .filter(Boolean) as any[]}
            pagination={false}
            rowKey="check"
            columns={[
              { title: 'Check', dataIndex: 'check' },
              { title: 'Valore', dataIndex: 'value' },
              { title: 'Soglia', dataIndex: 'threshold' },
              { title: 'Default', dataIndex: 'default' },
              { title: 'Δ', dataIndex: 'diff' },
              {
                title: 'Esito',
                dataIndex: 'pass',
                render: (_: any, r: any) => (
                  <span style={{ color: r.color }}>{r.pass ? 'OK' : 'NO'}</span>
                ),
              },
            ]}
          />
          <pre style={{ background: '#f6f6f6', padding: 10, marginTop: 20 }}>
            {JSON.stringify(result, null, 2)}
          </pre>
          {generateHeatmaps && result.BlurHeatmap && (
            <Image src={`data:image/png;base64,${result.BlurHeatmap}`} alt="Blur heatmap" />
          )}
          {generateHeatmaps && result.GlareHeatmap && (
            <Image src={`data:image/png;base64,${result.GlareHeatmap}`} alt="Glare heatmap" />
          )}
        </div>
      )}
    </Card>
  );
};

export default QualityChecker;
