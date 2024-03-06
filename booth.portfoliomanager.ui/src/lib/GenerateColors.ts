
export interface ColorRangeInfo {
	colorStart: number;
	colorEnd: number;
	useEndAsStart: boolean
}

type ColorScale = (n: number) => string

export function generateColors(dataLength: number, colorScale: ColorScale, colorRangeInfo: ColorRangeInfo) {

	const { colorStart, colorEnd } = colorRangeInfo;
	const colorRange = colorEnd - colorStart;
	const intervalSize = colorRange / dataLength;

	const colorArray: string[] = [];
	for (let i = 0; i < dataLength; i++) {
		const colorPoint = calculatePoint(i, intervalSize, colorRangeInfo);
		colorArray.push(colorScale(colorPoint));
	}
  
	return colorArray;
}


function calculatePoint(index: number, intervalSize: number, colorRangeInfo: ColorRangeInfo): number {

	const { colorStart, colorEnd, useEndAsStart } = colorRangeInfo;

	return (useEndAsStart ? (colorEnd - (index * intervalSize)) : (colorStart + (index * intervalSize)));
}